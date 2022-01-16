// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.UI.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly NuagesSignInManager _signInManager;
      
        public LoginWith2faModel(
            NuagesSignInManager signInManager,
          ILogger<LoginWith2faModel> logger)
        {
            _signInManager = signInManager;
        }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }
       
    }
}
