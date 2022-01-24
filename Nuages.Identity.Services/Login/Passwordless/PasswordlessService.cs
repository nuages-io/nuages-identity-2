using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Sender.API.Sdk;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.Services.Login.Passwordless;

public class PasswordlessService : IPasswordlessService
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signinManager;
    private readonly IMessageSender _messageSender;
    private readonly NuagesIdentityOptions _options;

    public PasswordlessService(NuagesUserManager userManager, NuagesSignInManager signinManager, 
        IMessageSender messageSender,
        IOptions<NuagesIdentityOptions> options)
    {
        _userManager = userManager;
        _signinManager = signinManager;
        _messageSender = messageSender;
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

        await _userManager.UpdateSecurityStampAsync(user);

        await _signinManager.SignInAsync(user, false);

        return new PasswordlessResultModel
        {
            Success = true
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

        var res = await GetPasswordlessUrl(user);

        if (res.Success)
        {
            await _messageSender.SendEmailUsingTemplateAsync(user.Email, "Passwordless_Login", new Dictionary<string, string>
            {
                { "Link", res.Url },
                { "AppName", _options.Name}
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
}

public class GetPasswordlessUrlResultModel
{
    public bool Success { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Message { get; set; }
}