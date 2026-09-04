using Dapper;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace SOLTIUS_Web_API_Add_On.Database.Initializers
{
    public class LogDatabaseInitializer
    {
        private readonly string _connectionString;

        public LogDatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitializeAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                await CreateApiLogTable(conn);
                await CreateSyncLogTable(conn);
            }
        }

        private async Task CreateApiLogTable(SqlConnection conn)
        {
            string sql = @"
                IF OBJECT_ID('api_logs','U') IS NULL
                CREATE TABLE api_logs
                (
                    id BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
                    ts DATETIME2 NOT NULL DEFAULT(GETDATE()),
                    method VARCHAR(10) NOT NULL,
                    path VARCHAR(500) NOT NULL,
                    status INT NOT NULL,
                    client_ip VARCHAR(45) NULL,
                    uid VARCHAR(100) NULL,
                    detail NVARCHAR(500) NULL,
                    duration_ms INT NULL
                );
                CREATE INDEX IX_api_logs_ts ON api_logs(ts);
                CREATE INDEX IX_api_logs_status ON api_logs(status);";

            await conn.ExecuteAsync(sql);
        }

        private async Task CreateSyncLogTable(SqlConnection conn)
        {
            string sql = @"
                IF OBJECT_ID('sync_logs','U') IS NULL
                CREATE TABLE sync_logs
                (
                    id BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
                    ts DATETIME2 NOT NULL DEFAULT(GETDATE()),
                    uid VARCHAR(100) NULL,
                    card_code VARCHAR(30) NULL,
                    doc_type VARCHAR(50) NULL,
                    doc_entry VARCHAR(50) NULL,
                    item_code VARCHAR(30) NULL,
                    quantity DECIMAL(19,6) NULL,
                    price DECIMAL(19,6) NULL,
                    warehouse VARCHAR(20) NULL,
                    status VARCHAR(20) NOT NULL,
                    error_source VARCHAR(100) NULL,
                    error_message NVARCHAR(MAX) NULL
                );
                CREATE INDEX IX_sync_logs_ts ON sync_logs(ts);
                CREATE INDEX IX_sync_logs_status ON sync_logs(status);";

            await conn.ExecuteAsync(sql);
        }
    }
}
