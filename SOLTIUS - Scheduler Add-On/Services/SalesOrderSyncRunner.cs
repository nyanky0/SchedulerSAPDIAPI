using SOLTIUS_Scheduler_Add_On.Model;
using SOLTIUS_Scheduler_Add_On.Services;
using SOLTIUS_Scheduler_Add_On.UI;
using System;
using System.Collections.Generic;

namespace SOLTIUS_Scheduler_Add_On.Services
{
    /// <summary>
    /// Engine sinkronisasi Sales Order dari staging ke SAP, tanpa ketergantungan UI.
    /// </summary>
    public static class SalesOrderSyncRunner
    {
        /// <summary>
        /// Menjalankan sinkronisasi semua SO pending (process_status = 0).
        /// </summary>
        /// <returns>Jumlah dokumen yang gagal.</returns>
        public static int RunPendingSync(AppConfig config, bool isDryRun)
        {
            if (config == null)
                throw new ArgumentNullException("config", "Profil tidak ditemukan.");

            string connString = ConfigService.BuildStagingConnectionString(config);
            if (string.IsNullOrEmpty(connString))
                throw new InvalidOperationException(
                    "Staging hanya mendukung SQL Server. Profil aktif memakai tipe '" + config.ExternalDBType + "'.");

            var dbService = new DatabaseService(connString);
            List<PendingSalesOrder> orders = dbService.LoadPendingSalesOrders();

            if (orders.Count == 0) return 0;

            int failedCount = 0;
            using (var sapService = new SapSyncService())
            {
                if (!isDryRun)
                    sapService.ConnectToDIAPI(config);

                foreach (var order in orders)
                {
                    // --- Skip if retry limit exceeded ---
                    if (dbService.IsRetryLimitExceeded(order.HeaderId))
                    {
                        dbService.MarkAsExceededRetryLimit(order.HeaderId);
                        LogSync(dbService, order, "Failed", null, "Skipped: max retry limit exceeded");
                        continue;
                    }

                    try
                    {
                        if (isDryRun)
                        {
                            LogSync(dbService, order, "Success", "DRY-RUN", "Validasi berhasil (Mode Simulasi)");
                        }
                        else
                        {
                            string docEntry = sapService.ExecuteSalesOrderSync(order);
                            LogSync(dbService, order, "Success", docEntry, "-");
                            dbService.UpdateSalesOrderStatus(order.HeaderId, 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;

                        // Check if retry limit reached
                        int newRetryCount = GetCurrentRetryCount(dbService, order.HeaderId) + 1;
                        string errMsg = ex.Message;
                        if (newRetryCount >= GetMaxRetryCount())
                        {
                            errMsg = "[DEAD-LETTER] " + ex.Message;
                            dbService.UpdateSalesOrderStatus(order.HeaderId, 2, ex.Message);
                            dbService.MarkAsExceededRetryLimit(order.HeaderId);
                        }
                        else
                        {
                            dbService.UpdateSalesOrderStatus(order.HeaderId, 2, ex.Message);
                        }

                        LogSync(dbService, order, "Failed", null, errMsg);
                    }
                }
            }

            return failedCount;
        }

        private static int GetCurrentRetryCount(DatabaseService dbService, long headerId)
        {
            // Use IsRetryLimitExceeded as proxy; actual count comes from DB
            // For simplicity, rely on the existing retrycount field
            return 0; // The DB already tracks retrycount, used in MarkAsExceededRetryLimit
        }

        private static int GetMaxRetryCount()
        {
            // Default: 5. Can be changed here or read from config.
            return 5;
        }

        /// <summary>
        /// Log sync result with UID. Multi-line: logs once per order (not per line).
        /// </summary>
        private static void LogSync(DatabaseService dbService, PendingSalesOrder order, string status, string docEntry, string errorMessage)
        {
            try
            {
                var log = new SyncLogModel
                {
                    DocType = "Sales Order",
                    DocEntry = docEntry ?? "",
                    CardCode = order.CardCode ?? "",
                    ItemCode = order.Lines.Count > 0 ? order.Lines[0].ItemCode : "",
                    Quantity = order.Lines.Count > 0 ? (int)order.Lines[0].Quantity : 0,
                    Price = order.Lines.Count > 0 ? (double)order.Lines[0].Price : 0,
                    WarehouseCode = order.Lines.Count > 0 ? order.Lines[0].Warehouse : "",
                    Status = status,
                    ErrorSource = status == "Failed" ? "SAP Validation" : "-",
                    ErrorMessage = errorMessage ?? "-",
                    CreatedAt = DateTime.Now
                };

                if (dbService != null)
                    dbService.SaveLogToDatabase(log);
            }
            catch
            {
                // Logging failure must not kill the sync
            }
        }
    }
}
