using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using Nuages.Identity.Services.Login;

// ReSharper disable VirtualMemberCallInConstructor

namespace Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class NuagesApplicationUser<TKey> : IdentityUser<TKey> where TKey : IEquatable<TKey>
{
    public NuagesApplicationUser()
    {
        Id = StringToKey(Guid.NewGuid().ToString());
        SecurityStamp = Guid.NewGuid().ToString();
    }
    
    protected virtual TKey StringToKey(string id)
    {
        return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id)!;
    }
    
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

    public string? PreferredMfaMethod { get; set; }
}