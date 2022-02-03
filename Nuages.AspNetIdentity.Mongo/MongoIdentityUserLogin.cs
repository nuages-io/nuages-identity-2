using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Mongo;

public class MongoIdentityUserLogin<TKey> : IdentityUserLogin<TKey> where TKey : IEquatable<TKey>
{
    public string Id { get; set; } = "";
}