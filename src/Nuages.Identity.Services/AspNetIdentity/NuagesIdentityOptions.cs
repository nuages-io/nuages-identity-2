using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.AspNetIdentity;

public class NuagesIdentityOptions
{
    public string Name { get; set; } = string.Empty;

    public string Authority { get; set; } = string.Empty;
    public string[]? Audiences { get; set; }
    public bool ValidateAudience { get; set; }
    
    public PasswordOptions Password { get; set; } = new();

    public bool EnableAutoPasswordExpiration { get; set; }
    public int AutoExpirePasswordDelayInDays { get; set; } = 90;

    public bool SupportsLoginWithEmail { get; set; } = true;

    public bool AutoConfirmExternalLogin { get; set; }
    
    public bool EnablePasswordHistory { get; set; }
    public int PasswordHistoryCount { get; set; } = 5;

 
    public string? SigningKey { get; set; }
    public string? EncryptionKey { get; set; }
    public int KeySize { get; set; } = 2048;
}