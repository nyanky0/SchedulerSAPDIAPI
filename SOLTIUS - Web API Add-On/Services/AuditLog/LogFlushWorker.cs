using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SOLTIUS_Web_API_Add_On.Services.AuditLog
{
    public class LogFlushWorker : BackgroundService
    {
        private readonly Channel<ApiLogEntry> _apiChannel;
        private readonly Channel<SyncLogEntry> _syncChannel;
        private readonly string _connectionString;
        private readonly ILogger<LogFlushWorker> _logger;

        private const int BatchSize = 100;
        private const int FlushIntervalMs = 5000;

        public LogFlushWorker(
            Channel<ApiLogEntry> apiChannel,
            Channel<SyncLogEntry> syncChannel,
            LogDatabaseOptions options,
            ILogger<LogFlushWorker> logger)
        {
            _apiChannel = apiChannel;
            _syncChannel = syncChannel;
            _connectionString = options.ConnectionString;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LogFlushWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(FlushIntervalMs, stoppingToken);
                    await FlushAllAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "LogFlushWorker flush error.");
                }
            }

            // Final flush on shutdown
            try { await FlushAllAsync(CancellationToken.None); }
            catch { /* best effort */ }

            _logger.LogInformation("LogFlushWorker stopped.");
        }

        private async Task FlushAllAsync(CancellationToken ct)
        {
            var apiBatch = new List<ApiLogEntry>();
            while (apiBatch.Count < BatchSize && _apiChannel.Reader.TryRead(out var entry))
            {
                apiBatch.Add(entry);
            }

            var syncBatch = new List<SyncLogEntry>();
            while (syncBatch.Count < BatchSize && _syncChannel.Reader.TryRead(out var entry))
            {
                syncBatch.Add(entry);
            }

            if (apiBatch.Count == 0 && syncBatch.Count == 0) return;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync(ct);

                    if (apiBatch.Count > 0)
                        await FlushApiLogs(conn, apiBatch);

                    if (syncBatch.Count > 0)
                        await FlushSyncLogs(conn, syncBatch);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to flush logs to database. Entries lost: api={ApiCount}, sync={SyncCount}",
                    apiBatch.Count, syncBatch.Count);
            }
        }

        private static async Task FlushApiLogs(SqlConnection conn, List<ApiLogEntry> entries)
        {
            string sql = @"INSERT INTO api_logs (ts, method, path, status, client_ip, uid, detail, duration_ms)
                           VALUES (@Ts, @Method, @Path, @Status, @ClientIp, @Uid, @Detail, @DurationMs)";

            foreach (var e in entries)
            {
                await conn.ExecuteAsync(sql, new
                {
                    Ts = e.Ts,
                    Method = e.Method ?? "",
                    Path = e.Path ?? "",
                    Status = e.Status,
                    ClientIp = e.ClientIp ?? "",
                    Uid = e.Uid ?? "",
                    Detail = e.Detail ?? "",
                    DurationMs = e.DurationMs
                });
            }
        }

        private static async Task FlushSyncLogs(SqlConnection conn, List<SyncLogEntry> entries)
        {
            string sql = @"INSERT INTO sync_logs
                           (ts, uid, card_code, doc_type, doc_entry, item_code, quantity, price, warehouse, status, error_source, error_message)
                           VALUES (@Ts, @Uid, @CardCode, @DocType, @DocEntry, @ItemCode, @Quantity, @Price, @Warehouse, @Status, @ErrorSource, @ErrorMessage)";

            foreach (var e in entries)
            {
                await conn.ExecuteAsync(sql, new
                {
                    Ts = e.Ts,
                    Uid = e.Uid ?? "",
                    CardCode = e.CardCode ?? "",
                    DocType = e.DocType ?? "",
                    DocEntry = e.DocEntry ?? "",
                    ItemCode = e.ItemCode ?? "",
                    Quantity = e.Quantity,
                    Price = e.Price,
                    Warehouse = e.Warehouse ?? "",
                    Status = e.Status ?? "",
                    ErrorSource = e.ErrorSource ?? "",
                    ErrorMessage = e.ErrorMessage ?? ""
                });
            }
        }
    }
}
