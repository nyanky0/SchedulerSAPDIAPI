using Dapper;
using MySql.Data.MySqlClient;
using SOLTIUS_Web_API_Add_On.Database.Interfaces;
using SOLTIUS_Web_API_Add_On.Models.Configuration;

namespace SOLTIUS_Web_API_Add_On.Database.Initializers
{
    public class MySqlDatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        public MySqlDatabaseInitializer(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task InitializeAsync(DBConfig config)
        {
            using var connection = (MySqlConnection)_connectionFactory.CreateConnection(config);
            await connection.OpenAsync();

            await CreateSalesOrderHeader(connection);
            await CreateSalesOrderDetail(connection);
        }

        private async Task CreateSalesOrderHeader(MySqlConnection connection)
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS sales_order_header
            (
                id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                cardcode VARCHAR(30) NOT NULL,
                cardname VARCHAR(200) NOT NULL,
                docdate DATETIME NOT NULL,
                docduedate DATETIME NOT NULL,
                taxdate DATETIME NOT NULL,
                remarks VARCHAR(254),
                process_status TINYINT NOT NULL DEFAULT 0,
                retrycount INT NOT NULL DEFAULT 0,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME,
                processed_at DATETIME,
                errormessage TEXT
            );";

            await connection.ExecuteAsync(sql);
        }

        private async Task CreateSalesOrderDetail(MySqlConnection connection)
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS sales_order_detail
            (
                id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                header_id BIGINT NOT NULL,
                linenum INT NOT NULL,
                itemcode VARCHAR(30) NOT NULL,
                itemname VARCHAR(200) NOT NULL,
                warehouse VARCHAR(20),
                quantity DECIMAL(19,6) NOT NULL,
                price DECIMAL(19,6),
                process_status TINYINT NOT NULL DEFAULT 0,
                retrycount INT NOT NULL DEFAULT 0,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME,
                processed_at DATETIME,
                errormessage TEXT
            );";

            await connection.ExecuteAsync(sql);
        }
    }
}
