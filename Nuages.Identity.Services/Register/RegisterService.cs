using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Sender.API.Sdk;

namespace Nuages.Identity.Services.Register;

public class RegisterService : IRegisterService
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly IStringLocalizer _localizer;
    private readonly IMessageSender _messageSender;
    private readonly NuagesIdentityOptions _options;

    public RegisterService(NuagesUserManager userManager, NuagesSignInManager signInManager, IStringLocalizer localizer, 
        IMessageSender messageSender, IOptions<NuagesIdentityOptions> options)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _localizer = localizer;
     
        _messageSender = messageSender;
        _options = options.Value;
    }
    
    public async Task<RegisterResultModel> Register(RegisterModel model)
    {
        if (model.Password != model.PasswordConfirm)
        {
            return new RegisterResultModel
            {
                Success = false,
                Errors =new List<string>{ _localizer["register.passwordDoesNotMatch"]}
            };
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            return new RegisterResultModel
            {
                Success = false,
                Errors =new List<string> {_localizer["register.userEmailAlreadyExists"]}
            };
        }

        user = new NuagesApplicationUser
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = model.Email,
            UserName = model.Email
        };
        
        var res = await _userManager.CreateAsync(user, model.Password);

        if (res.Succeeded)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var url = $"{_options.Authority}/Account/ConfirmEmail?code={code}&userId={user.Id}";

            await _messageSender.SendEmailUsingTemplateAsync(model.Email, "Confirm_Email", new Dictionary<string, string>
            {
                { "Link", url },
                { "AppName", _options.Name}
            });
        
            if (_userManager.Options.SignIn.RequireConfirmedEmail)
            {
                return new RegisterResultModel
                {
                    ShowConfirmationMessage = true,
                    Success = true
                };
            }
            
            await _signInManager.SignInAsync(user, false);

            return new RegisterResultModel
            {
                Success = true
            };
        }

        return new RegisterResultModel
        {
            Success = false,
            Errors = res.Errors.Select( e=> _localizer[$"identity.{e.Code}"].Value).ToList()
        };
    }
}

public interface IRegisterService
{
    Task<RegisterResultModel> Register(RegisterModel model);
}

// ReSharper disable once ClassNeverInstantiated.Global
public class RegisterModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirm { get; set; } = string.Empty;
    public string? RecaptchaToken { get; set; }
}
public class RegisterResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool ShowConfirmationMessage { get; set; }
}