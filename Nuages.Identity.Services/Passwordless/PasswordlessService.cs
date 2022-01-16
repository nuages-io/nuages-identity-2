using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.Services.Passwordless;

public class PasswordlessService : IPasswordlessService
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signinManager;
    private readonly NuagesIdentityOptions _options;

    public PasswordlessService(NuagesUserManager userManager, NuagesSignInManager signinManager, IOptions<NuagesIdentityOptions> options)
    {
        _userManager = userManager;
        _signinManager = signinManager;
        _options = options.Value;
    }
    
    public async Task<GetPasswordlessUrlResultModel> GetPasswordlessUrl(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");
        
        var token = await _userManager.GenerateUserTokenAsync(user, "PasswordlessLoginProvider",
            "passwordless-auth");

        var baseUrl = _options.Authority;
        if (!baseUrl!.EndsWith("/"))
            baseUrl += "/";
        
        var url = $"{baseUrl}api/account/passwordless?token={token}&userId={userId}";

        return new GetPasswordlessUrlResultModel
        {
            Success = true,
            Url = url
        };
    }

    public async Task<PasswordlessResultModel> LoginPasswordLess(string token, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var isValid = await _userManager.VerifyUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth", token);
        if (!isValid)
        {
            return new PasswordlessResultModel
            {
                Success = false
            };
        }

        await _userManager.UpdateSecurityStampAsync(user);

        await _signinManager.SignInAsync(user, isPersistent: false);

        return new PasswordlessResultModel
        {
            Success = true
        };
    }
}

public class PasswordlessResultModel
{
    public bool Success { get; set; }
}

public interface IPasswordlessService
{
    Task<GetPasswordlessUrlResultModel> GetPasswordlessUrl(string userId);
    Task<PasswordlessResultModel> LoginPasswordLess(string token, string userId);
}

public class GetPasswordlessUrlResultModel
{
    public bool Success { get; set; }
    public string Url { get; set; } = string.Empty;
}