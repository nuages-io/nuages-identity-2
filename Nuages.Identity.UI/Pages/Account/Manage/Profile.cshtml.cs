using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web;
using Nuages.Web.Exceptions;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

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
        if (user == null)
            throw new NotFoundException("UserNotFound");

        CurrentUser = user;

        return Page();
    }

    public NuagesApplicationUser CurrentUser { get; set; }
}