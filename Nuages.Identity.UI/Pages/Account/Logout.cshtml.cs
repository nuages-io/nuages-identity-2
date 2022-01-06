// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.UI.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly NuagesSignInManager _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(NuagesSignInManager signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return Redirect("~/");
            }
        }
    }
}
