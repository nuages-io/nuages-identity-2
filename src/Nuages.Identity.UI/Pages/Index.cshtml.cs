﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages;

[Authorize(AuthenticationSchemes = "Identity.Application")]
public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        return Redirect("~/account/manage");
    }
}