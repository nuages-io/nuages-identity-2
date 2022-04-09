// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;
    private readonly NuagesSignInManager _signInManager;

    public LogoutModel(NuagesSignInManager signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet(string returnUrl = null)
    {
        try
        {            
            _logger.LogInformation("SignOutAsync");
            await _signInManager.SignOutAsync();
            
            _logger.LogInformation("YO! User logged out.");
            _logger.LogInformation($"LocalRedirect returnUrl = {returnUrl == null}");
            if (returnUrl != null)
            {
                _logger.LogInformation($"Redirect to {returnUrl}");
                return Redirect(returnUrl);
            }
                
            _logger.LogInformation("Redirect to root");
            return Redirect("/");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
}