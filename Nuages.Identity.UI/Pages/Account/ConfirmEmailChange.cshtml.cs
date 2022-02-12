// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Manage;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages.Account;

public class ConfirmEmailChangeModel : PageModel
{
    private readonly IChangeEmailService _changeEmailService;
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;

    public ConfirmEmailChangeModel(NuagesUserManager userManager, NuagesSignInManager signInManager,
        IChangeEmailService changeEmailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _changeEmailService = changeEmailService;
    }


    [TempData] public bool Success { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
    {
        if (userId == null || email == null || code == null) return RedirectToPage("/Index");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        email = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(email));

        var res = await _changeEmailService.ChangeEmailAsync(userId, email, code);

        if (!res.Success)
        {
            Success = false;
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);

        Success = true;

        return Page();
    }
}