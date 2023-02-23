// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.Email;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly IConfirmEmailService _confirmEmailService;
    private readonly ILogger<ConfirmEmailModel> _logger;

    private readonly IStringLocalizer _localizer;

    public ConfirmEmailModel(IStringLocalizer localizer, IConfirmEmailService confirmEmailService, ILogger<ConfirmEmailModel> logger)
    {
        _localizer = localizer;
        _confirmEmailService = confirmEmailService;
        _logger = logger;
    }

    [TempData] public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string code)
    {
        try
        {
            if (userId == null || code == null) return RedirectToPage("/Index");

            var res = await _confirmEmailService.Confirm(userId, code);

            StatusMessage = res ? _localizer["confirmEmail:success"] : _localizer["confirmEmail:error"];

            return Page();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            throw;
        }
    }
}