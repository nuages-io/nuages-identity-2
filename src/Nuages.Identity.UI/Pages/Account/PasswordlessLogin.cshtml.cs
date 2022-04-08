using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.Login.Passwordless;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

public class PasswordlessLogin : PageModel
{
    private readonly IPasswordlessService _passwordlessService;
    private readonly ILogger<PasswordlessLogin> _logger;

    public PasswordlessLogin(IPasswordlessService passwordlessService, ILogger<PasswordlessLogin> logger)
    {
        _passwordlessService = passwordlessService;
        _logger = logger;
    }

    public virtual async Task<IActionResult> OnGet(string token, string userId, string? returnUrl = null)
    {
        try
        {
            var res = await _passwordlessService.LoginPasswordLess(token, userId);

            if (res.Success)
                return Redirect("/");

            if (res.Result.RequiresTwoFactor) return Redirect($"/account/loginwith2fa?returnUrl={WebUtility.UrlEncode(returnUrl)}");

            return Unauthorized();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
}