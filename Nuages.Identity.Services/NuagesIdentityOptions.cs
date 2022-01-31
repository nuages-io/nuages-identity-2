using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services;

[ExcludeFromCodeCoverage]
public class NuagesIdentityOptions
{
    public string Name { get; set; } = string.Empty;
    
    public string Authority { get; set; } = string.Empty;
    public string[]? Audiences { get; set; } 
    
    public PasswordOptions Password { get; set; } = new ();

    public bool SupportsStartEnd { get; set; }
    
    public bool EnableAutoPasswordExpiration { get; set; }
    public int AutoExpirePasswordDelayInDays { get; set; } = 90;

    public bool EnableUserLockout { get; set; } = true;

    public bool SupportsUserName { get; set; } = true;
    public bool RequireConfirmedEmail { get; set; }

    public bool AutoConfirmExternalLogin { get; set; }
}