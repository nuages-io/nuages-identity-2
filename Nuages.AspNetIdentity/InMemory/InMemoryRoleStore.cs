using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.InMemory;

// ReSharper disable once ClassNeverInstantiated.Global
public class InMemoryRoleStore<TRole, TKey> : 
IRoleClaimStore<TRole>,
IQueryableRoleStore<TRole>
where TRole : IdentityRole<TKey>
where TKey : IEquatable<TKey>
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
    
    public static readonly List<TRole> RolesCollection = new();
    private static readonly List<IdentityRoleClaim<TKey>> RoleClaims = new();
    
    public Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
    {
        RolesCollection.Add(role);

        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
    {
        RolesCollection.Remove(role);
        
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<string?> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id.ToString());
    }

    public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;

        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;

        return Task.CompletedTask;
    }

    public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        var role = RolesCollection.SingleOrDefault(t => t.Id.Equals(roleId));

        return Task.FromResult(role)!;
    }

    public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = RolesCollection.SingleOrDefault(t => t.NormalizedName == normalizedRoleName);

        return Task.FromResult(role)!;
    }

    public Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = new())
    {
        var list = RoleClaims.Where(c => c.RoleId.Equals(role.Id)).Select(c =>
            new Claim(c.ClaimType, c.ClaimValue)
        ).ToList();
        
        return Task.FromResult((IList<Claim>) list);
    
    }

    public Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = new())
    {
        RoleClaims.Add(new IdentityRoleClaim<TKey>
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
            RoleClaims.FirstOrDefault(
                uc => uc.RoleId.Equals(role.Id) && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value);
        if (entity != null)
        {
            RoleClaims.Remove(entity);
        }
        
        return Task.CompletedTask;
    }

    public IQueryable<TRole> Roles => RolesCollection.AsQueryable();
}