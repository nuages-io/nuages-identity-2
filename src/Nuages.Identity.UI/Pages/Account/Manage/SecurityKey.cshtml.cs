// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class SecurityKeyModel : PageModel
{
    private readonly NuagesUserManager _userManager;

    public string Username { get; set; }
    
    public SecurityKeyModel(NuagesUserManager userManager)
    {
        _userManager = userManager;
    }
    public async Task<ActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) 
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");


        Username = user.UserName;

        return Page();
    }

   
}