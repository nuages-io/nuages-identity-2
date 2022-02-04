using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores.InMemory;

// ReSharper disable once ClassNeverInstantiated.Global
public class InMemoryRoleStore<TRole, TKey> : RoleStoreBase <TRole, TKey>,
IRoleClaimStore<TRole>,
IQueryableRoleStore<TRole>
where TRole : IdentityRole<TKey>
where TKey : IEquatable<TKey>
{
    [ExcludeFromCodeCoverage]
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
    
    private readonly List<TRole> _rolesCollection = new();
    private readonly List<IdentityRoleClaim<TKey>> _roleClaims = new();
    
    private static TKey StringToKey(string id)
    {
        return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id)!;
    }
    
    public Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
    {
        role.Id ??= StringToKey(Guid.NewGuid().ToString());
        
        _rolesCollection.Add(role);

        return Task.FromResult(IdentityResult.Success);
    }

    public override Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
    {
        _rolesCollection.Remove(role);
        
        return Task.FromResult(IdentityResult.Success);
    }

    public Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = new())
    {
        _roleClaims.Add(new IdentityRoleClaim<TKey>
        {
            RoleId = role.Id,
            ClaimType = claim.Type,
            ClaimValue = claim.Value
        });

        return Task.CompletedTask;
    }

    public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = new())
    {
        var entity =
            _roleClaims.FirstOrDefault(
                uc => uc.RoleId.Equals(role.Id) && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value);
        if (entity != null)
        {
            _roleClaims.Remove(entity);
        }
        
        return Task.CompletedTask;
    }

    public override IQueryable<TRole> Roles => _rolesCollection.AsQueryable();
    public override IQueryable<IdentityRoleClaim<TKey>> RolesClaims => _roleClaims.AsQueryable();
}