using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores;

public abstract class NoSqlRoleStoreBase<TRole, TKey> : IDisposable where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    private bool _disposed;
    public abstract IQueryable<TRole> Roles { get; }
    protected abstract IQueryable<IdentityRoleClaim<TKey>> RolesClaims { get; }

    public void Dispose()
    {
        _disposed = true;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().Name);
    }

    public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var role = Roles.SingleOrDefault(t => t.Id.Equals(roleId));

        return Task.FromResult(role)!;
    }

    public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        // ReSharper disable once SpecifyStringComparison
        var role = Roles.SingleOrDefault(t => t.NormalizedName.ToUpper() == normalizedRoleName.ToUpper());

        return Task.FromResult(role)!;
    }

    public Task<string?> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(role.Id.ToString());
    }

    public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(role.Name);
    }

    public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(role.NormalizedName);
    }

    public Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = new())
    {
        var list = RolesClaims.Where(c => c.RoleId.Equals(role.Id)).Select(c =>
            new Claim(c.ClaimType, c.ClaimValue)
        ).ToList();

        return Task.FromResult((IList<Claim>)list);
    }

    public async Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        role.NormalizedName = normalizedName;

        await UpdateAsync(role, cancellationToken);
    }

    public async Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        role.Name = roleName;

        await UpdateAsync(role, cancellationToken);
    }


    public abstract Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken);
}