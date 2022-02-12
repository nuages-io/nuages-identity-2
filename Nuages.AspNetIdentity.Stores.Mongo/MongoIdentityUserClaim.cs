namespace Nuages.AspNetIdentity.Stores.Mongo;

public class MongoIdentityUserClaim<TKey> where TKey : IEquatable<TKey>
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";

    public TKey UserId { get; set; } = default!;
}