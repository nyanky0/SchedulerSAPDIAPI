using Microsoft.AspNetCore.Mvc;
using SOLTIUS_Web_API_Add_On.Models.Transaction;
using SOLTIUS_Web_API_Add_On.Repositories;
using SOLTIUS_Web_API_Add_On.Services;
using SOLTIUS_Web_API_Add_On.Services.AuditLog;

namespace SOLTIUS_Web_API_Add_On.Controllers
{
    [Route("api/[controller]")]
    public class SalesOrderController : CustomApiControllerBase
    {
        private readonly ISalesOrderService _service;
        private readonly ISalesOrderRepository _repository;
        private readonly IAuditLogService _auditLog;

        public SalesOrderController(
            ISalesOrderService service,
            ISalesOrderRepository repository,
            IAuditLogService auditLog)
        {
            _service = service;
            _repository = repository;
            _auditLog = auditLog;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesOrder([FromBody] SalesOrderHeader salesOrder)
        {
            if (salesOrder == null)
                return BadRequest(new { success = false, message = "Invalid payload." });

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Validation failed.", errors = ModelState });

            if (salesOrder.DocumentLines == null || salesOrder.DocumentLines.Count == 0)
                return BadRequest(new { success = false, message = "DocumentLines is Required." });

            try
            {
                await _service.SaveSalesOrderAsync(salesOrder);

                _auditLog.LogApiRequest(
                    "POST", "/api/SalesOrder", 200,
                    HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "",
                    "",
                    $"cardCode={salesOrder.CardCode} lines={salesOrder.DocumentLines.Count}");

                return Ok(new
                {
                    success = true,
                    message = "Sales Order Received"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
