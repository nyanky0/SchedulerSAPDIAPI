using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SOLTIUS_Web_API_Add_On.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class CustomApiControllerBase : ControllerBase
    {
    }
}
