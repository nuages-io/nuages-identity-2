using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores.Mongo;

public class MongoIdentityUserToken<TKey> : IdentityUserToken<TKey> where TKey : IEquatable<TKey>
{
    public string Id { get; set; } = "";
}