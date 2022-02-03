using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.AspNetIdentity.Mongo;

public class MongoIdentityUserRole<TKey> : IdentityUserRole<TKey> where TKey : IEquatable<TKey>
{
    public string Id { get; set; } = "";
}