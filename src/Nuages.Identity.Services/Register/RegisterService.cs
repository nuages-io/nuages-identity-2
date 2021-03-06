using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;

namespace Nuages.Identity.Services.Register;

public class RegisterService : IRegisterService
{
    private readonly IStringLocalizer _localizer;
    private readonly IMessageService _messageService;
    private readonly IIdentityEventBus _identityEventBus;
    private readonly NuagesIdentityOptions _options;
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;

    public RegisterService(NuagesUserManager userManager, NuagesSignInManager signInManager, IStringLocalizer localizer,
        IMessageService messageService, IOptions<NuagesIdentityOptions> options, IIdentityEventBus identityEventBus)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _localizer = localizer;

        _messageService = messageService;
        _identityEventBus = identityEventBus;
        _options = options.Value;
    }

    public async Task<RegisterResultModel> Register(RegisterModel model)
    {
        if (model.Password != model.PasswordConfirm)
            return new RegisterResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["register.passwordDoesNotMatch"] }
            };

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
            return new RegisterResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["register.userEmailAlreadyExists"] }
            };

        user = new NuagesApplicationUser<string>
        {
            Email = model.Email,
            UserName = model.Email
        };

        var res = await _userManager.CreateAsync(user, model.Password);

        if (res.Succeeded)
        {
            await _identityEventBus.PutEvent(IdentityEvents.Register, user);
            
            if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var url = $"{_options.Authority}Account/ConfirmEmail?code={code}&userId={user.Id}";
                
                _messageService.SendEmailUsingTemplate(model.Email, "Confirm_Email", new Dictionary<string, string>
                {
                    { "Link", url }
                });
                
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
            Errors = res.Errors.Localize(_localizer)
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
}

public class RegisterResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool ShowConfirmationMessage { get; set; }
}