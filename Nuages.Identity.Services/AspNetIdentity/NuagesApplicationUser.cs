
using Microsoft.AspNetCore.Identity;
using Nuages.Identity.Services.Login;

namespace Nuages.Identity.Services.AspNetIdentity;


public class NuagesApplicationUser : IdentityUser<string>
{
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    public DateTime? LastLogin { get; set; }
    public int LoginCount { get; set; }

    public bool UserMustChangePassword { get; set; }
    
    public DateTime CreatedOn { get; set; }
    
    public DateTime? LastPasswordChangedDate { get; set; }
    public bool EnableAutoExpirePassword { get; set; } = true;

    public FailedLoginReason LastFailedLoginReason { get; set; }
    
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    
    public bool LockoutMessageSent { get; set; }
}