// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        return Redirect("/account/manage/profile");
    }

}