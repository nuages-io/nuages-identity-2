// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

// ReSharper disable once InconsistentNaming
public class LoginWithSecurityKeyModel : PageModel
{
    public string? Username { get; set; }
    
    private readonly NuagesSignInManager _signInManager;

    public LoginWithSecurityKeyModel(
        NuagesSignInManager signInManager,
        ILogger<LoginWith2faModel> logger)
    {
        _signInManager = signInManager;
    }

    public bool RememberMe { get; set; }

    public string ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
    {
        if (string.IsNullOrEmpty(returnUrl))
            returnUrl = "~/";

        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

        ReturnUrl = returnUrl;
        RememberMe = rememberMe;

        Username = user.UserName;
        
        return Page();
    }

    
}