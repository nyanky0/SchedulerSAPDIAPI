using SOLTIUS_Web_API_Add_On.Models.Transaction;

namespace SOLTIUS_Web_API_Add_On.Repositories
{
    public interface ISalesOrderRepository
    {
        Task InsertSalesOrderAsync(SalesOrderHeader salesOrder);
    }
}
