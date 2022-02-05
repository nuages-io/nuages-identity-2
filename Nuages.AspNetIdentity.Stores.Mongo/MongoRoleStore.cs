using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.AspNetIdentity.Stores.Mongo;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global
public class MongoRoleStore<TRole, TKey> : RoleStoreBase<TRole, TKey>,
IRoleClaimStore<TRole>,
IQueryableRoleStore<TRole>
where TRole : IdentityRole<TKey>
where TKey : IEquatable<TKey>
{
    private readonly IdentityErrorDescriber _errorDescriber = new ();
    
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // ReSharper disable once StaticMemberInGenericType
    public static ReplaceOptions ReplaceOptions { get; } = new();

    // ReSharper disable once StaticMemberInGenericType
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static DeleteOptions DeleteOptions { get; } = new();

    [ExcludeFromCodeCoverage]
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public MongoRoleStore(IOptions<MongoIdentityOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.Database);

        RolesCollection = database.GetCollection<TRole>("AspNetRoles");
        RoleClaimsCollection = database.GetCollection<IdentityRoleClaim<TKey>>("AspNetRoleClaims");
    }
    
    private  IMongoCollection<TRole> RolesCollection { get; }
    private  IMongoCollection<IdentityRoleClaim<TKey>> RoleClaimsCollection { get; }
    
    private static TKey ConvertIdFromString(string id)
    {
        return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id)!;
    }
    
    public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
    {
        role.Id = ConvertIdFromString(ObjectId.GenerateNewId().ToString());

        await SetNormalizedRoleNameAsync(role, role.Name.ToUpper(), cancellationToken);
        
        await RolesCollection.InsertOneAsync(role, null, cancellationToken);

        return IdentityResult.Success;
    }

    public override async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
    {
        var replaceOneResult = await RolesCollection.ReplaceOneAsync(r => r.Id.Equals(role.Id), role, ReplaceOptions, cancellationToken);

        return ReturnUpdateResult(replaceOneResult);
    }

    [ExcludeFromCodeCoverage]
    private IdentityResult ReturnUpdateResult(ReplaceOneResult replaceOneResult)
    {
        if (replaceOneResult.IsAcknowledged || replaceOneResult.ModifiedCount != 0L)
            return IdentityResult.Success;

        return IdentityResult.Failed(_errorDescriber.ConcurrencyFailure());
    }

    public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
    {
        var result = await RolesCollection.DeleteOneAsync(m => m.Id.Equals(role.Id), DeleteOptions, cancellationToken);

        return ReturnDeleteResult(result);
    }

    [ExcludeFromCodeCoverage]
    private IdentityResult ReturnDeleteResult(DeleteResult result)
    {
        if (result.IsAcknowledged || result.DeletedCount != 0L)
            return IdentityResult.Success;

        return IdentityResult.Failed(_errorDescriber.ConcurrencyFailure());
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
            await RoleClaimsCollection.DeleteOneAsync( c => c.Id.Equals(entity.Id), DeleteOptions, cancellationToken);
        }
        
    }

    public override IQueryable<TRole> Roles => RolesCollection.AsQueryable();
    protected override IQueryable<IdentityRoleClaim<TKey>> RolesClaims => RoleClaimsCollection.AsQueryable();
}