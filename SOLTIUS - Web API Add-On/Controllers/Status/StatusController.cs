using Microsoft.AspNetCore.Mvc;
using SOLTIUS_Web_API_Add_On.Controllers;
using SOLTIUS_Web_API_Add_On.Models.Status;
using SOLTIUS_Web_API_Add_On.Services.Status;

namespace SOLTIUS_Web_API_Add_On.Controllers.Status
{
    [Route("api/[controller]")]
    public class StatusController : CustomApiControllerBase
    {
        private readonly IStatusService _statusService;

        public StatusController(IStatusService statusService)
        {
            _statusService = statusService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            ApiStatus status = await _statusService.GetStatusAsync();

            if (status.Configured && status.DatabaseConnection)
            {
                return Ok(status);
            }

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                status);
        }
    }
}