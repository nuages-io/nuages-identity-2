using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.UI.Setup;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages.Account;

// ReSharper disable once InconsistentNaming
public class LoginWithSMS : PageModel
{
    private readonly UIOptions _options;
    private readonly NuagesSignInManager _signInManager;
    private readonly ILogger<LoginWithSMS> _logger;

    public LoginWithSMS(NuagesSignInManager signInManager, IOptions<UIOptions> options, ILogger<LoginWithSMS> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
        _options = options.Value;
    }

    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGet(string? returnUrl = null)
    {
        try
        {
            if (!_options.EnablePhoneFallback) return Forbid();

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

            ReturnUrl = returnUrl ?? "~/";

            return Page();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
}