using SOLTIUS_Web_API_Add_On.Models.Transaction;

namespace SOLTIUS_Web_API_Add_On.Services
{
    public interface ISalesOrderService
    {
        Task SaveSalesOrderAsync(SalesOrderHeader salesOrder);
    }
}
