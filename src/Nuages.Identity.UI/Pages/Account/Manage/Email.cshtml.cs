// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class EmailModel : PageModel
{
    private readonly NuagesUserManager _userManager;
    private readonly ILogger<EmailModel> _logger;

    public EmailModel(NuagesUserManager userManager, ILogger<EmailModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [TempData] public string Email { get; set; } = string.Empty;

    [TempData] public bool EmailVerified { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var user = await _userManager.FindByIdAsync(User.Sub()!);
            Email = user.Email;
            EmailVerified = user.EmailConfirmed;

            return Page();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }
}