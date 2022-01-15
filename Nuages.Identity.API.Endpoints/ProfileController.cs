using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.API.Services;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.API.Endpoints;

[ExcludeFromCodeCoverage]

[Route("api/profile")]
public class ProfileController
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly NuagesUserManager _userManager;
    private readonly RoleManager<NuagesApplicationRole> _roleManager;

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AdminController> _logger;
    private readonly IChangePasswordService _changePasswordService;
    private readonly IChangeEmailService _changeEmailService;
    private readonly IChangePhoneNumberService _changePhoneNumberService;
    private readonly IChangeUserNameService _changeUserNameService;
    private readonly IMFAService _mfaService;

    
    public ProfileController(IHttpContextAccessor contextAccessor, NuagesUserManager userManager, RoleManager<NuagesApplicationRole> roleManager,
        IWebHostEnvironment env, ILogger<AdminController> logger,
        IChangePasswordService changePasswordService,
        IChangeEmailService changeEmailService,
        IChangePhoneNumberService changePhoneNumberService,
        IChangeUserNameService changeUserNameService,
        IMFAService mfaService)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _roleManager = roleManager;
        
        _env = env;
        _logger = logger;
        _changePasswordService = changePasswordService;
        _changeEmailService = changeEmailService;
        _changePhoneNumberService = changePhoneNumberService;
        _changeUserNameService = changeUserNameService;
        _mfaService = mfaService;
    }
    
    [HttpPost("changeEmail")]
    public async Task<bool> ChangeEmail()
    {
        return await Task.FromResult(true);
    }
    
    [HttpPost("changeUsername")]
    public async Task<bool> ChangeUserName()
    {
        return await Task.FromResult(true);
    }
    
    [HttpPost("changePhoneNumber")]
    public async Task<bool> ChengePhoneNumber()
    {
        return await Task.FromResult(true);
    }
    
    [HttpPost("changePassword")]
    public async Task<bool> ChangePassword()
    {
        // if (!await _roleManager.RoleExistsAsync("Admin2"))
        // {
        //     var role = new NuagesApplicationRole
        //     {
        //         Id = ObjectId.GenerateNewId().ToString(),
        //         Name = "Admin2",
        //         NormalizedName = "ADMIN"
        //     };
        //     
        //     await _roleManager.CreateAsync(role);
        //
        //     await _roleManager.AddClaimAsync(role, new Claim("Test", "Value"));
        // }
        
        //var user = await _userManager.FindByNameAsync(_contextAccessor.HttpContext.User.Identity.Name);
        
        // var r = await _userManager.IsInRoleAsync(user,"Admin");
        // if (!r)
        // {
        //     await _userManager.AddToRoleAsync(user, "Admin");
        // }
        
        //var r = await _userManager.IsInRoleAsync(user,"Admin");
        
        return await Task.FromResult(true);
    }
    
    [HttpPost("addMfa")]
    public async Task<bool> AddAuthenticator()
    {
        return await Task.FromResult(true);
    }
    
    [HttpDelete("removeMfa")]
    public async Task<bool> RemoaveMfa()
    {
        return await Task.FromResult(true);
    }
    
    [HttpPost("resetRecoveryCodes")]
    public async Task<bool> ResetRecoveryCodes()
    {
        return await Task.FromResult(true);
    }
}