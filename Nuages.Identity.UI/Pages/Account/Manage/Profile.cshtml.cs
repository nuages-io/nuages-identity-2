using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Nuages.AspNetIdentity.Core;
using Nuages.Web;
using Nuages.Web.Exceptions;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class Profile : PageModel
{
    private readonly NuagesUserManager _userManager;

    public Profile(NuagesUserManager userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.FindByIdAsync(User.Sub()!);

        CurrentUser = user ?? throw new NotFoundException("UserNotFound");

        return Page();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public NuagesApplicationUser? CurrentUser { get; set; }
}