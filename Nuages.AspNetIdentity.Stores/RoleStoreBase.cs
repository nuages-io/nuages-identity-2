using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores;

public abstract class RoleStoreBase<TRole, TKey> where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    public abstract IQueryable<TRole> Roles { get; }
    protected abstract IQueryable<IdentityRoleClaim<TKey>> RolesClaims { get; }
    
    public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        var role = Roles.SingleOrDefault(t => t.Id.Equals(roleId));

        return Task.FromResult(role)!;
    }
    
    public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        // ReSharper disable once SpecifyStringComparison
        var role = Roles.SingleOrDefault(t => t.NormalizedName.ToUpper() == normalizedRoleName.ToUpper());

        return Task.FromResult(role)!;
    }
    
    public Task<string?> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id.ToString());
    }

    public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }
    
    public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }
    
    public Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = new())
    {
        var list = RolesClaims.Where(c => c.RoleId.Equals(role.Id)).Select(c =>
            new Claim(c.ClaimType, c.ClaimValue)
        ).ToList();
        
        return Task.FromResult((IList<Claim>) list);
    }
    
    public async Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;

        await UpdateAsync(role, cancellationToken);
    }
    
    public async Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;

        await UpdateAsync(role, cancellationToken);
    }

    

     public abstract Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken);
    
}