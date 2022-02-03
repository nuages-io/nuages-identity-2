using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.AspNetIdentity;



// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

[Authorize(AuthenticationSchemes = NuagesIdentityConstants.EmailNotVerifiedScheme)]
public class EmailNotConfirmed : PageModel
{
    public void OnGet()
    {
      
    }
}