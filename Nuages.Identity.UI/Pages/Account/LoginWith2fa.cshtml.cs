// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Fido2;
using Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

// ReSharper disable once InconsistentNaming
public class LoginWith2faModel : PageModel
{
    private readonly NuagesSignInManager _signInManager;
    private readonly IFido2Service? _fido2Service;
    
    public LoginWith2faModel(
        NuagesSignInManager signInManager,
        IEnumerable<IFido2Service> fido2Services,
        ILogger<LoginWith2faModel> logger)
    {
        _signInManager = signInManager;
        _fido2Service = fido2Services.FirstOrDefault();
    }

    public bool RememberMe { get; set; }

    public string ReturnUrl { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(bool rememberMe, bool redirectToPreferred = false, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(returnUrl))
            returnUrl = "~/";

        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

        if (_fido2Service != null && redirectToPreferred)
        {
            if (user.PreferredMfaMethod == "SecurityKeys")
            {
                var canRedirectToKeys = await _fido2Service.HasSecurityKeys(Encoding.UTF8.GetBytes(user.Id));
                if (canRedirectToKeys)
                {
                    Response.Redirect($"/account/loginWithSecurityKey?rememberMe={rememberMe}&returnUrl={returnUrl}");
                }
            }
        }

        ReturnUrl = returnUrl;
        RememberMe = rememberMe;

        return Page();
    }
}