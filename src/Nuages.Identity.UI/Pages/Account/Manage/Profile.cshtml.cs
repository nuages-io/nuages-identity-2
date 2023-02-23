
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web;
using Nuages.Web.Exceptions;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class Profile : PageModel
{
    private readonly NuagesUserManager _userManager;
    private readonly ILogger<Profile> _logger;

    public Profile(NuagesUserManager userManager, ILogger<Profile> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public NuagesApplicationUser<string>? CurrentUser { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var user = await _userManager.FindByIdAsync(User.Sub()!);

            CurrentUser = user ?? throw new NotFoundException("UserNotFound");

            return Page();
        }
        catch (Exception e)
        {
            _logger.LogError(e,"{Message}",e.Message);

            throw;
        }
    }
}