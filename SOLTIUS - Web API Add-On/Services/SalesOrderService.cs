using SOLTIUS_Web_API_Add_On.Models.Transaction;
using SOLTIUS_Web_API_Add_On.Repositories;

namespace SOLTIUS_Web_API_Add_On.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly ISalesOrderRepository _repository;

        public SalesOrderService(ISalesOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task SaveSalesOrderAsync(SalesOrderHeader salesOrder)
        {
            if (salesOrder == null)
                throw new ArgumentNullException(nameof(salesOrder));

            if (string.IsNullOrWhiteSpace(salesOrder.CardCode))
                throw new Exception("CardCode is required.");

            if (salesOrder.DocumentLines == null || salesOrder.DocumentLines.Count == 0)
                throw new Exception("DocumentLines is required.");

            await _repository.InsertSalesOrderAsync(salesOrder);
        }
    }
}
