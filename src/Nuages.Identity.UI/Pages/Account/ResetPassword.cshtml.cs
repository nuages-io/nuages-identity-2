// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<ResetPasswordModel> _logger;

    public ResetPasswordModel(IHttpContextAccessor contextAccessor, IStringLocalizer localizer, ILogger<ResetPasswordModel> logger)
    {
        _contextAccessor = contextAccessor;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<ActionResult> OnGet(string code = null, bool expired = false)
    {
        try
        {
            ViewData["expired"] = expired;
            ViewData["email"] = "";

            if (!expired)
            {
                if (code == null) return BadRequest("A code must be supplied for password reset.");

                ViewData["Instructions"] = _localizer["resetPassword:instructions"];
                ViewData["Title"] = _localizer["resetPassword.title"];
                ViewData["Submit"] = _localizer["resetPassword:reset"];

                var res = await _contextAccessor.HttpContext!.AuthenticateAsync(NuagesIdentityConstants
                    .ResetPasswordScheme);
                if (res.Succeeded)
                {
                    var email = res.Principal!.FindFirstValue(ClaimTypes.Email);

                    ViewData["email"] = email;
                }

                ViewData["code"] = code;
            }
            else
            {
                var res = await _contextAccessor.HttpContext!.AuthenticateAsync(NuagesIdentityConstants
                    .PasswordExpiredScheme);
                if (!res.Succeeded)
                    return Unauthorized();

                ViewData["Instructions"] = _localizer["passwordExpired:instructions"];
                ViewData["Title"] = _localizer["passwordExpired.title"];
                ViewData["Submit"] = _localizer["passwordExpired:submit"];

                var email = res.Principal!.FindFirstValue(ClaimTypes.Email);

                ViewData["email"] = email;

                var newCode = res.Principal!.FindFirstValue(ClaimTypes.UserData);

                ViewData["code"] = newCode;
            }

            return Page();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
}