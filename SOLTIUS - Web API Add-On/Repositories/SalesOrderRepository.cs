using Dapper;
using SOLTIUS_Web_API_Add_On.Database.Interfaces;
using SOLTIUS_Web_API_Add_On.Models.Configuration;
using SOLTIUS_Web_API_Add_On.Models.Transaction;
using SOLTIUS_Web_API_Add_On.Services.Configuration;
using System.Data.Common;

namespace SOLTIUS_Web_API_Add_On.Repositories
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IConfigurationService _configurationService;

        public SalesOrderRepository(IDatabaseConnectionFactory connectionFactory, IConfigurationService configurationService)
        {
            _connectionFactory = connectionFactory;
            _configurationService = configurationService;
        }

        public async Task InsertSalesOrderAsync(SalesOrderHeader salesOrder)
        {
            DBConfig config = _configurationService.GetDatabaseConfig();
            bool isMySql = config.DBType == DatabaseType.MySql;

            using DbConnection conn = _connectionFactory.CreateConnection(config);
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // --- Header ---
                string headerSql = isMySql
                    ? @"INSERT INTO sales_order_header
                        (cardcode, cardname, docdate, docduedate, taxdate, remarks, process_status, created_at)
                        VALUES (@CardCode, @CardName, @DocDate, @DocDueDate, @TaxDate, @Remarks, 0, NOW());
                        SELECT LAST_INSERT_ID();"
                    : @"INSERT INTO sales_order_header
                        (cardcode, cardname, docdate, docduedate, taxdate, remarks, process_status, created_at)
                        VALUES (@CardCode, @CardName, @DocDate, @DocDueDate, @TaxDate, @Remarks, 0, GETDATE());
                        SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                long headerId = await conn.ExecuteScalarAsync<long>(headerSql, salesOrder, transaction);

                // --- Details ---
                int lineNum = 0;
                foreach (var line in salesOrder.DocumentLines)
                {
                    string detailSql = isMySql
                        ? @"INSERT INTO sales_order_detail
                            (header_id, linenum, itemcode, itemname, warehouse, quantity, price, process_status, created_at)
                            VALUES (@HeaderId, @LineNum, @ItemCode, @ItemName, @Warehouse, @Quantity, @Price, 0, NOW());"
                        : @"INSERT INTO sales_order_detail
                            (header_id, linenum, itemcode, itemname, warehouse, quantity, price, process_status, created_at)
                            VALUES (@HeaderId, @LineNum, @ItemCode, @ItemName, @Warehouse, @Quantity, @Price, 0, GETDATE());";

                    await conn.ExecuteAsync(detailSql, new
                    {
                        HeaderId = headerId,
                        LineNum = ++lineNum,
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemDescription,
                        Warehouse = line.WarehouseCode,
                        Quantity = line.Quantity,
                        Price = line.Price
                    }, transaction);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
