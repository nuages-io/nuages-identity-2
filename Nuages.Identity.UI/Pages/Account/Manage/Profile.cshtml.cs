using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.UI.AWS;
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

    // ReSharper disable once MemberCanBePrivate.Global
    public NuagesApplicationUser<string>? CurrentUser { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            
            var user = await _userManager.FindByIdAsync(User.Sub()!);

            CurrentUser = user ?? throw new NotFoundException("UserNotFound");

            return Page();
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
       
    }
}