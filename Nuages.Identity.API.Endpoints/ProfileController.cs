using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.API.Endpoints;

[ExcludeFromCodeCoverage]
//[Authorize(Roles = "Admin")]
[Route("api/profile")]
public class ProfileController
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly NuagesUserManager _userManager;
    private readonly RoleManager<NuagesApplicationRole> _roleManager;

    public ProfileController(IHttpContextAccessor contextAccessor, NuagesUserManager userManager, RoleManager<NuagesApplicationRole> roleManager)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _roleManager = roleManager;
    }
    // public async Task<bool> ChangeEmail()
    // {
    //     return await Task.FromResult(true);
    // }
    //
    // public async Task<bool> ChangeUserName()
    // {
    //     return await Task.FromResult(true);
    // }
    //
    // public async Task<bool> ChengePhoneNumber()
    // {
    //     return await Task.FromResult(true);
    // }
    
    [HttpPost("changePassword")]
    public async Task<bool> ChangePassword()
    {
        // if (!await _roleManager.RoleExistsAsync("Admin"))
        // {
        //     await _roleManager.CreateAsync(new NuagesApplicationRole
        //     {
        //         Id = ObjectId.GenerateNewId().ToString(),
        //         Name = "Admin",
        //         NormalizedName = "ADMIN"
        //     });
        // }
        
        var user = await _userManager.FindByNameAsync(_contextAccessor.HttpContext.User.Identity.Name);
        
        // var r = await _userManager.IsInRoleAsync(user,"Admin");
        // if (!r)
        // {
        //     await _userManager.AddToRoleAsync(user, "Admin");
        // }
        
        var r = await _userManager.IsInRoleAsync(user,"Admin");
        
        return await Task.FromResult(r);
    }
    
    // public async Task<bool> AddAuthenticator()
    // {
    //     return await Task.FromResult(true);
    // }
    //
    // public async Task<bool> RemoveAuthenticator()
    // {
    //     return await Task.FromResult(true);
    // }
    //
    // public async Task<bool> ResetRecoveryCodes()
    // {
    //     return await Task.FromResult(true);
    // }
}