using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.Services.Login.Passwordless;

public class PasswordlessService : IPasswordlessService
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signinManager;
    private readonly IMessageService _messageService;
    private readonly IStringLocalizer _localizer;
    private readonly NuagesIdentityOptions _options;

    public PasswordlessService(NuagesUserManager userManager, NuagesSignInManager signinManager, 
        IMessageService messageService, IStringLocalizer localizer,
        IOptions<NuagesIdentityOptions> options)
    {
        _userManager = userManager;
        _signinManager = signinManager;
        _messageService = messageService;
        _localizer = localizer;
        _options = options.Value;
    }
    
    public async Task<GetPasswordlessUrlResultModel> GetPasswordlessUrl(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        return await GetPasswordlessUrl(user);
    }
    
    private async Task<GetPasswordlessUrlResultModel> GetPasswordlessUrl(NuagesApplicationUser user)
    {
      
        var token = await _userManager.GenerateUserTokenAsync(user, "PasswordlessLoginProvider",
            "passwordless-auth");

        var baseUrl = _options.Authority;
        
        var url = $"{baseUrl}/account/passwordlessLogin?token={token}&userId={user.Id}";

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

        var result = await _signinManager.CustomPreSignInCheck(user);
        if (!result.Succeeded)
        {
            return new PasswordlessResultModel
            {
                Result = result,
                Reason = user.LastFailedLoginReason,
                Message = _localizer[LoginService.GetMessageKey(user.LastFailedLoginReason)],
                Success = false
            };
        }
        
        await _userManager.UpdateSecurityStampAsync(user);

        result = await _signinManager.CustomSignInOrTwoFactorAsync(user, false);

        return new PasswordlessResultModel
        {
            Success = result.Succeeded,
            Result = result
        };
    }

    public async Task<StartPasswordlessResultModel> StartPasswordless(StartPasswordlessModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new StartPasswordlessResultModel
            {
                Success = true //Fake success
            };
        }

        var result = await _signinManager.CustomPreSignInCheck(user);
        if (!result.Succeeded)
        {
            return new StartPasswordlessResultModel
            {
                Result = result,
                Reason = user.LastFailedLoginReason,
                Message = _localizer[LoginService.GetMessageKey(user.LastFailedLoginReason)],
                Success = false
            };
        }
        
        var res = await GetPasswordlessUrl(user);

        if (res.Success)
        {
            _messageService.SendEmailUsingTemplate(user.Email, "Passwordless_Login", new Dictionary<string, string>
            {
                { "Link", res.Url }
            });

            
            Console.WriteLine(res.Url);

            return new StartPasswordlessResultModel
            {
                Success = true
            };
        }
       
        return new StartPasswordlessResultModel
        {
            Success = false,
            Message = res.Message
        };
    }
}

public class PasswordlessResultModel
{
    public bool Success { get; set; }
    public SignInResult Result { get; set; } = null!;
    public FailedLoginReason? Reason { get; set; }
    public string? Message { get; set; }
}

public interface IPasswordlessService
{
    Task<GetPasswordlessUrlResultModel> GetPasswordlessUrl(string userId);
    Task<PasswordlessResultModel> LoginPasswordLess(string token, string userId);
    Task<StartPasswordlessResultModel> StartPasswordless(StartPasswordlessModel model);
}

public class StartPasswordlessModel
{
    public string Email { get; set; } = string.Empty;
    public string? RecaptchaToken { get; set; }
}

public class StartPasswordlessResultModel
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public SignInResult Result { get; set; } = null!;
    public FailedLoginReason? Reason { get; set; }
}

public class GetPasswordlessUrlResultModel
{
    public bool Success { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Message { get; set; }
}