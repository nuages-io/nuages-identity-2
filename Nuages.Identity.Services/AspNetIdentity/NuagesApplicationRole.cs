using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable once ClassNeverInstantiated.Global
public class NuagesApplicationRole<TKey> : IdentityRole<TKey> where TKey : IEquatable<TKey>
{
    protected virtual TKey StringToKey(string id)
    {
        return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id)!;
    }
    
    public NuagesApplicationRole()
    {
        Id = StringToKey(Guid.NewGuid().ToString());
    }
}