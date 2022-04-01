using Microsoft.AspNetCore.Mvc.RazorPages;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages.Connect;

public class VerifyAccepted : PageModel
{
    public bool Success { get; set; }

    public void OnGet(bool success = true)
    {
        Success = success;
    }
}