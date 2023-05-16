// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Manage;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class EnableAuthenticatorModel : PageModel
{
    private readonly IMFAService _mfaService;
    private readonly ILogger<EnableAuthenticatorModel> _logger;
    private readonly NuagesUserManager _userManager;

    public EnableAuthenticatorModel(
        NuagesUserManager userManager,
        IMFAService mfaService, ILogger<EnableAuthenticatorModel> logger)
    {
        _userManager = userManager;
        _mfaService = mfaService;
        _logger = logger;
    }

    public string SharedKey { get; set; }
    public string AuthenticatorUri { get; set; }
    public string Username { get; set; }
    
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var url = await _mfaService.GetMFAUrlAsync(user!.Id);

            SharedKey = FormatKey(url.Key);
            AuthenticatorUri = url.Url;

            Username = User.Identity!.Name;
            
            return Page();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            throw;
        }
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length) result.Append(unformattedKey.AsSpan(currentPosition));

        return result.ToString().ToLowerInvariant();
    }
}