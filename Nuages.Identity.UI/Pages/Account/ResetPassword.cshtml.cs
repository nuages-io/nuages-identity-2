// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.UI.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public ResetPasswordModel(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        
        public async Task<ActionResult> OnGet(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }

            ViewData["email"] = "";
                
            var res = await _contextAccessor.HttpContext!.AuthenticateAsync(NuagesIdentityConstants.ResetPasswordScheme);
            if (res.Succeeded)
            {
                var claim = res.Principal!.Claims.Single(u => u.Type == ClaimTypes.Email);
                
                ViewData["email"] = claim.Value;
            }

            ViewData["code"] = code; ;
                
            return Page();
        }
    }
}
