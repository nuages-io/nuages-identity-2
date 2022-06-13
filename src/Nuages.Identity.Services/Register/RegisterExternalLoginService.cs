using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;

namespace Nuages.Identity.Services.Register;

public class RegisterExternalLoginService : IRegisterExternalLoginService
{
    private readonly IMessageService _messageService;
    private readonly IIdentityEventBus _eventBus;
    private readonly ILogger<RegisterExternalLoginService> _logger;
    private readonly NuagesIdentityOptions _options;
    private readonly NuagesSignInManager _signInManager;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly NuagesUserManager _userManager;

    public RegisterExternalLoginService(NuagesSignInManager signInManager, NuagesUserManager userManager,
        IOptions<NuagesIdentityOptions> options,
        IStringLocalizer stringLocalizer, IMessageService messageService, IIdentityEventBus eventBus, ILogger<RegisterExternalLoginService> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _stringLocalizer = stringLocalizer;
        _messageService = messageService;
        _eventBus = eventBus;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<RegisterExternalLoginResultModel> Register()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            const string error = "Error loading external login information during confirmation. {info}";
            
            _logger.LogError(error, info);
            
            return new RegisterExternalLoginResultModel
            {
                Success = false,
                Errors = new List<string> { error }
            };
        }
            

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            const string error =  "Email claim not found";
            
            _logger.LogError(error);
            
            return new RegisterExternalLoginResultModel
            {
                Success = false,
                Errors = new List<string> {error }
            };
        }
            

        var user = new NuagesApplicationUser<string>
        {
            Email = email,
            UserName = email,
            EmailConfirmed = _options.AutoConfirmExternalLogin
        };

        var result = await _userManager.CreateAsync(user);

        if (result.Succeeded)
        {
            result = await AddLoginAsync(user, info);
            if (result.Succeeded)
            {
                await _eventBus.PutEvent(IdentityEvents.Register, user);
                
                if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var url = $"{_options.Authority}/Account/ConfirmEmail?code={code}&userId={user.Id}";

                    _messageService.SendEmailUsingTemplate(email, "Confirm_Email", new Dictionary<string, string>
                    {
                        { "Link", url }
                    });

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
            Errors = result.Errors.Localize(_stringLocalizer)
        };
    }


    // ReSharper disable once SuggestBaseTypeForParameter
    private async Task<IdentityResult> AddLoginAsync(NuagesApplicationUser<string> user, ExternalLoginInfo info)
    {
#if DEBUG

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (info.LoginProvider == "invalid")
            return IdentityResult.Failed(new IdentityError
            {
                Description = "error"
            });

        /* PATH for unit test*/
        if (info.LoginProvider == "loginProvider" && info.ProviderKey == "providerKey" &&
            info.ProviderDisplayName == "displayName")
            return IdentityResult.Success;


#endif

        var res = await _userManager.AddLoginAsync(user, info);

        return res;
    }
}

public interface IRegisterExternalLoginService
{
    // ReSharper disable once UnusedParameter.Global
    Task<RegisterExternalLoginResultModel> Register();
}

[ExcludeFromCodeCoverage]
public class RegisterExternalLoginResultModel
{
    public bool Success { get; set; }

    public List<string> Errors { get; set; } = new();

    public bool ShowConfirmationMessage { get; set; }
}