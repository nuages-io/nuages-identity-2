using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.UI.AWS;


// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

[AllowAnonymous]
public class Login : PageModel
{
    private readonly NuagesIdentityOptions _nuagesIdentityOptions;
    private readonly NuagesSignInManager _signInManager;
    private readonly IStringLocalizer _stringLocalizer;

    public Login(IStringLocalizer stringLocalizer, IOptions<NuagesIdentityOptions> nuagesIdentityOptions,
        NuagesSignInManager signInManager)
    {
        _stringLocalizer = stringLocalizer;
        _signInManager = signInManager;
        _nuagesIdentityOptions = nuagesIdentityOptions.Value;
    }

    public string? ReturnUrl { get; set; }
    public string UserNamePlaceHolder { get; set; } = "Email";

    public List<AuthenticationScheme> ExternalLogins { get; set; } = new();

    public async Task<ActionResult> OnGetAsync(string? returnUrl = null)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            
            if (User.Identity is { IsAuthenticated: true })
            {
                if (string.IsNullOrEmpty(returnUrl))
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Redirect(returnUrl!);
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");

            UserNamePlaceHolder =
                _stringLocalizer[
                    _nuagesIdentityOptions.SupportsLoginWithEmail ? "Login:Mode:userNameEmail" : "Login:Mode:email"];

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return Page();
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }

    }
}