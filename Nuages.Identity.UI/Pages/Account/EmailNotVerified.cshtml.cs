using Microsoft.AspNetCore.Mvc.RazorPages;


// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

//[Authorize(AuthenticationSchemes = NuagesIdentityConstants.EmailNotVerifiedScheme)]
public class EmailNotVerified : PageModel
{
    // private readonly ITenantContext _tenantContext;
    // private readonly IStringLocalizer _stringLocalizer;
    //
     public string? RecaptchaSiteKey { get; set; }
    //
    // public EmailNotVerified(ITenantContext tenantContext, IStringLocalizer stringLocalizer)
    // {
    //     _tenantContext = tenantContext;
    //     _stringLocalizer = stringLocalizer;
    // }
    //
    // public async Task OnGet()
    // {
    //     var tenant = await _tenantContext.GetCurrentAsync();
    //     if (tenant == null)
    //         throw new NotAuthorizedException();
    //     
    //     RecaptchaSiteKey = tenant.RecaptchaSiteKey;
    //
    // }
}