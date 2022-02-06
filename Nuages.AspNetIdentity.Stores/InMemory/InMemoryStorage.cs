using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores.InMemory;

public class InMemoryStorage<TRole, TKey> : IInMemoryStorage<TRole>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    public List<TRole> Roles { get; set; } = new ();
}

public interface IInMemoryStorage<TRole>
{
    List<TRole> Roles { get; }
}