using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Nuages.Identity.Services.AspNetIdentity.Mongo;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global
public class MongoRoleStore<TRole, TKey> : 
IRoleClaimStore<TRole>,
IQueryableRoleStore<TRole>
where TRole : IdentityRole<TKey>
where TKey : IEquatable<TKey>
{
    private readonly IdentityErrorDescriber _errorDescriber = new ();
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public MongoRoleStore(IOptions<NuagesIdentityOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);

        RolesCollection = database.GetCollection<TRole>("Roles");
        RoleClaimsCollection = database.GetCollection<IdentityRoleClaim<TKey>>("RolesClaims");
    }
    
    private  IMongoCollection<TRole> RolesCollection { get; }
    private  IMongoCollection<IdentityRoleClaim<TKey>> RoleClaimsCollection { get; }
    
    public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
    {
        await RolesCollection.InsertOneAsync(role, null, cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
    {
        var replaceOneResult = await RolesCollection.ReplaceOneAsync(r => r.Id.Equals(role.Id), role, (ReplaceOptions?) null, cancellationToken);
        if (replaceOneResult.IsAcknowledged || replaceOneResult.ModifiedCount != 0L)
            return IdentityResult.Success;
        
        return IdentityResult.Failed(_errorDescriber.ConcurrencyFailure());
    }

    public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
    {
        var result = await RolesCollection.DeleteOneAsync(m => m.Id.Equals(role.Id), null, cancellationToken);
        
        if (result.IsAcknowledged || result.DeletedCount != 0L)
            return IdentityResult.Success;
        
        return IdentityResult.Failed(_errorDescriber.ConcurrencyFailure());
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
        var role = Roles.SingleOrDefault(t => t.Id.Equals(roleId));

        return Task.FromResult(role)!;
    }

    public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = Roles.SingleOrDefault(t => t.NormalizedName == normalizedRoleName);

        return Task.FromResult(role)!;
    }

    public Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = new())
    {
        var list = RoleClaimsCollection.AsQueryable().Where(c => c.RoleId.Equals(role.Id)).Select(c =>
            new Claim(c.ClaimType, c.ClaimValue)
        ).ToList();
        
        return Task.FromResult((IList<Claim>) list);
    
    }

    public async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = new())
    {
        await RoleClaimsCollection.InsertOneAsync(new IdentityRoleClaim<TKey>
        {
            RoleId = role.Id,
            ClaimType = claim.Type,
            ClaimValue = claim.Value
        }, null, cancellationToken);

    }

    public async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = new())
    {
        var entity =
            RoleClaimsCollection.AsQueryable().FirstOrDefault(
                uc => uc.RoleId.Equals(role.Id) && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value);
        if (entity != null)
        {
            await RoleClaimsCollection.DeleteOneAsync( c => c.Id.Equals(entity.Id), null, cancellationToken);
        }
        
    }

    public IQueryable<TRole> Roles => RolesCollection.AsQueryable();
}