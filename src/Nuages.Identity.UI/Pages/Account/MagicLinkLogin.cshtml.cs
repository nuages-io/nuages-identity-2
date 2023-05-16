using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.Login.MagicLink;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

public class MagicLinkLogin : PageModel
{
    private readonly IMagicLinkService _magicLinkService;
    private readonly ILogger<MagicLinkLogin> _logger;

    public MagicLinkLogin(IMagicLinkService magicLinkService, ILogger<MagicLinkLogin> logger)
    {
        _magicLinkService = magicLinkService;
        _logger = logger;
    }

    public virtual async Task<IActionResult> OnGet(string token, string userId, string? returnUrl = null)
    {
        try
        {
            var res = await _magicLinkService.LoginMagicLink(token, userId);

            if (res.Success)
                return Redirect("/");

            if (res.Result.RequiresTwoFactor) return Redirect($"/account/loginwith2fa?returnUrl={WebUtility.UrlEncode(returnUrl)}");

            return Unauthorized();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            throw;
        }
    }
}