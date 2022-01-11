// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.UI.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly NuagesUserManager _userManager;
        private readonly IStringLocalizer _localizer;

        public ConfirmEmailModel(NuagesUserManager userManager, IStringLocalizer localizer)
        {
            _userManager = userManager;
            _localizer = localizer;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            
            StatusMessage = result.Succeeded ? _localizer["confirmEmail:success"] : _localizer["confirmEmail:error"];
            return Page();
        }
    }
}
