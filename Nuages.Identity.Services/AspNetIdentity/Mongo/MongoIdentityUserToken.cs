using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.AspNetIdentity.Mongo;

public class MongoIdentityUserToken<TKey> : IdentityUserToken<TKey> where TKey : IEquatable<TKey>
{
    public string Id { get; set; } = "";
}