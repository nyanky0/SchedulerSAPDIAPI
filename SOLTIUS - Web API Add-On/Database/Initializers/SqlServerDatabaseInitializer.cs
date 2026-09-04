using Dapper;
using Microsoft.Data.SqlClient;
using SOLTIUS_Web_API_Add_On.Database.Interfaces;
using SOLTIUS_Web_API_Add_On.Models.Configuration;

namespace SOLTIUS_Web_API_Add_On.Database.Initializers
{
    public class SqlServerDatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        public SqlServerDatabaseInitializer(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InitializeAsync(DBConfig config)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection(config);
            await connection.OpenAsync();

            await CreateSalesOrderHeader(connection);
            await CreateSalesOrderDetail(connection);
        }

        private async Task CreateSalesOrderHeader(SqlConnection connection)
        {
            string sql = @"
            IF OBJECT_ID('sales_order_header','U') IS NULL

            CREATE TABLE sales_order_header
            (
                id BIGINT NOT NULL IDENTITY(1,1),
                cardcode VARCHAR(30) NOT NULL,
                cardname VARCHAR(200) NOT NULL,
                docdate DATETIME NOT NULL,
                docduedate DATETIME NOT NULL,
                taxdate DATETIME NOT NULL,
                remarks VARCHAR(254),
                process_status TINYINT NOT NULL DEFAULT(0),
                retrycount INT NOT NULL DEFAULT(0),
                created_at DATETIME NOT NULL DEFAULT(GETDATE()),
                updated_at DATETIME NULL,
                processed_at DATETIME NULL,
                errormessage NVARCHAR(MAX) NULL
            );";

            await connection.ExecuteAsync(sql);
        }

        private async Task CreateSalesOrderDetail(SqlConnection connection)
        {
            string sql = @"
            IF OBJECT_ID('sales_order_detail','U') IS NULL

            CREATE TABLE sales_order_detail
            (
                id BIGINT NOT NULL IDENTITY(1,1),
                header_id BIGINT NOT NULL,
                linenum INT NOT NULL,
                itemcode VARCHAR(30) NOT NULL,
                itemname VARCHAR(200) NOT NULL,
                warehouse VARCHAR(20) NULL,
                quantity DECIMAL(19,6) NOT NULL,
                price DECIMAL(19,6) NULL,
                process_status TINYINT NOT NULL DEFAULT(0),
                retrycount INT NOT NULL DEFAULT(0),
                created_at DATETIME2 NOT NULL DEFAULT(GETDATE()),
                updated_at DATETIME NULL,
                processed_at DATETIME NULL,
                errormessage NVARCHAR(MAX) NULL
            );";

            await connection.ExecuteAsync(sql);
        }
    }
}
