

namespace Nuages.AspNetIdentity.Stores.Mongo;

public class MongoIdentityOptions
{
    public string ConnectionString { get; set; } = "";
    public string Database { get; set; } = "";
    
    public string Locale { get; set; } = "en";
}