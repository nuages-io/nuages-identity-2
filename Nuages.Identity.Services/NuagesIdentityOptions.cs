using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services;

public class NuagesIdentityOptions
{
    public string[]? Audiences { get; set; } 
    
    public PasswordOptions Password { get; set; } = new ();
}
