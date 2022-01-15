using MongoDB.Driver;

namespace Nuages.Identity.UI.Endpoints.OpenIdDict;

public class OpenIdDictOptions
{
    public string? SigningKey { get; set; }
    public string? EncryptionKey { get; set; }
    public string Database { get; set; }
    public string ConnectionString { get; set; }
}