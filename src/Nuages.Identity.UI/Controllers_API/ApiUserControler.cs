using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nuages.Identity.UI.Controllers_API;

[ApiController]
[Route("api/user")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ApiUserControler : Controller
{
    // GET
    [HttpGet("info")]
    public async Task<object> Index()
    {
        return new { Test = "Yo" };
    }
}