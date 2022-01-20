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

namespace Nuages.Identity.UI.Pages.Account.Manage
{
    [Authorize]
    public class UsernameModel : PageModel
    {
        private readonly NuagesUserManager _userManager;

        public UsernameModel(NuagesUserManager userManager)
        {
            _userManager = userManager;
        }

        [TempData] 
        public string Username { get; set; } = string.Empty;

        public void OnGetAsync()
        {
            Username = User.Identity!.Name;
        }
    }
}
