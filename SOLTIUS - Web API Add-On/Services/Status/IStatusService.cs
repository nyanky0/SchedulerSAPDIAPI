using SOLTIUS_Web_API_Add_On.Models.Status;

namespace SOLTIUS_Web_API_Add_On.Services.Status
{
    public interface IStatusService
    {
        Task<ApiStatus> GetStatusAsync();
    }
}
