using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.UI.AWS;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages.Account;

// ReSharper disable once InconsistentNaming
public class LoginWithSMS : PageModel
{
    private readonly UIOptions _options;
    private readonly NuagesSignInManager _signInManager;

    public LoginWithSMS(NuagesSignInManager signInManager, IOptions<UIOptions> options)
    {
        _signInManager = signInManager;
        _options = options.Value;
    }

    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGet(string? returnUrl = null)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            
            if (!_options.EnablePhoneFallback) return Forbid();

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

            ReturnUrl = returnUrl ?? "~/";

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