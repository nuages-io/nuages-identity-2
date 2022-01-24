using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Sender.API.Sdk;

namespace Nuages.Identity.Services.Register;

public class RegisterExternalLoginService : IRegisterExternalLoginService
{
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IMessageSender _messageSender;
    private readonly NuagesIdentityOptions _options;

    public RegisterExternalLoginService(NuagesSignInManager signInManager, NuagesUserManager userManager, IOptions<NuagesIdentityOptions> options,
            IStringLocalizer stringLocalizer, IMessageSender messageSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _stringLocalizer = stringLocalizer;
       _messageSender = messageSender;
        _options = options.Value;
    }
    
    public async Task<RegisterExternalLoginResultModel> Register(RegisterExternalLoginModel model)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return new RegisterExternalLoginResultModel
            {
                Success = false,
                Errors = new List<string> { "Error loading external login information during confirmation."}
            };
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            return new RegisterExternalLoginResultModel
            {
                Success = false,
                Errors = new List<string> { "Email claim not found"}
            };
        }
        
        var user = new NuagesApplicationUser
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };
        
        var result = await _userManager.CreateAsync(user);

        if (result.Succeeded)
        {
            result = await _userManager.AddLoginAsync(user, info);
            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
               
                var url = $"{_options.Authority}/Account/ConfirmEmail?code={code}&userId={user.Id}";

                await _messageSender.SendEmailUsingTemplateAsync(email, "Confirm_Email", new Dictionary<string, string>
                {
                    { "Link", url },
                    { "AppName", _options.Name}
                });
        
                if (_userManager.Options.SignIn.RequireConfirmedEmail)
                {
                    return new RegisterExternalLoginResultModel
                    {
                        ShowConfirmationMessage = true,
                        Success = true
                    };
                }
                
                await _signInManager.SignInAsync(user, false, info.LoginProvider);

                return new RegisterExternalLoginResultModel
                {
                    Success = true
                };
            }
            
        }

        return new RegisterExternalLoginResultModel
        {
            Success = false,
            Errors = result.Errors.Select( e=> _stringLocalizer[$"identity.{e.Code}"].Value).ToList()
        };
    }
}

public interface IRegisterExternalLoginService
{
    // ReSharper disable once UnusedParameter.Global
    Task<RegisterExternalLoginResultModel> Register(RegisterExternalLoginModel model);
}

// ReSharper disable once ClassNeverInstantiated.Global
public class RegisterExternalLoginModel
{
    public string? RecaptchaToken { get; set; }
}

public class RegisterExternalLoginResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool ShowConfirmationMessage { get; set; }
}