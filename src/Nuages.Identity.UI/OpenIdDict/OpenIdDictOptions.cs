namespace Nuages.Identity.UI.OpenIdDict;

public class OpenIdDictOptions
{
    public string? SigningKey { get; set; }
    public string? EncryptionKey { get; set; }
    public bool CreateDemoClients { get; set; } 
}