using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services;

public class NuagesIdentityOptions
{
    public string[]? Audiences { get; set; } 
    
    public PasswordOptions Password { get; set; } = new ();

    public OpenIdDict OpenIdDict { get; set; } = new ();
}


public class OpenIdDict
{
    public string? EncryptionKey { get; set; }
    public string? SigningKey { get; set; }
}