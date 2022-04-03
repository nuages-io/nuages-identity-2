using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

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

    [NotMapped]
    public PasswordHistory? PasswordHistory
    {
        get => string.IsNullOrWhiteSpace(PasswordHistoryJson) ? null : JsonSerializer.Deserialize<PasswordHistory>(PasswordHistoryJson);
        private set => PasswordHistoryJson = value != null ? JsonSerializer.Serialize(value) : null;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public string? PasswordHistoryJson { get; set; }

    public void AddPassword(string hash, int keepPasspordCount)
    {
        if (keepPasspordCount > 0)
        {
            var history = PasswordHistory ??= new PasswordHistory();

            history.AddPassword(hash, keepPasspordCount);

            PasswordHistory = history;
        }
        else
        {
            PasswordHistory = null;
        }
       
    }
}