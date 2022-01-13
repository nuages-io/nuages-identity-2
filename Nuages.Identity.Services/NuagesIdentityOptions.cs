using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services;

public class NuagesIdentityOptions
{
    public string[]? Audiences { get; set; } 
    
    public PasswordOptions Password { get; set; } = new ();

    public OpenIdDict OpenIdDict { get; set; } = new ();

    public bool SupportsStartEnd { get; set; }
    
    public bool SupportsAutoPasswordExpiration { get; set; }
    public int AutoExpirePasswordDelayInDays { get; set; }
    
    public bool SupportsUserLockout { get; set; }

    public int RequireConfirmedEmailGracePeriodInMinutes { get; set; } = 60;
    public int RequireConfirmedPhoneGracePeriodInMinutes { get; set; } = 60;

    public bool SupportsUserName { get; set; } = true;
    public bool RequireConfirmedEmail { get; set; }
    public bool RequireConfirmedPhoneNumber { get; set; }

    public bool ExternalLoginAutoEnrollIfEmailExists { get; set; }
    public bool ExternalLoginPersistent { get; set; }
}


public class OpenIdDict
{
    public string? EncryptionKey { get; set; }
    public string? SigningKey { get; set; }
}