using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.Services.AspNetIdentity;

public class KeyStoreFromConfiguration : IKeyStore
{
    private readonly NuagesIdentityOptions _options;

    public KeyStoreFromConfiguration(IOptions<NuagesIdentityOptions> options)
    {
        _options = options.Value;
    }

    public RsaSecurityKey? GetSigningKey()
    {
        var xml = _options.SigningKey;
        
        return !string.IsNullOrEmpty(xml) ? LoadRsaSecurityKey(xml) : null;
    }

    public RsaSecurityKey? GetEncryptionKey()
    {
        var xml = _options.EncryptionKey;
        
        return !string.IsNullOrEmpty(xml) ? LoadRsaSecurityKey(xml) : null;
    }

    public RsaSecurityKey CreateSigningKey()
    {
        return CreateRsaSecurityKey(_options.KeySize);
    }

    public RsaSecurityKey CreateEncryptionKey()
    {
        return CreateRsaSecurityKey(_options.KeySize);
    }

    public void SaveSigningKey(RsaSecurityKey key)
    {
        var xmlNewKey = key.Rsa.ToXmlString(true);

        Console.WriteLine(new string('-', 150));
        Console.WriteLine("The following SigningKey has been created and should be saved and provided by the configuration system (Nuages:OpenIdDict:SigningKey)");
        Console.WriteLine(new string('-', 150));
        Console.WriteLine(xmlNewKey);
        Console.WriteLine(new string('-', 150));
    }

    public void SaveEncryptionKey(RsaSecurityKey key)
    {
        var newKeyXml = key.Rsa.ToXmlString(true);

        Console.WriteLine(new string('-', 150));
        Console.WriteLine("The following EncryptionKey has been created and should be saved and provided by the configuration system (Nuages:OpenIdDict:EncryptionKey)");
        Console.WriteLine(new string('-', 150));
        Console.WriteLine(newKeyXml);
        Console.WriteLine(new string('-', 150));
    }

    public static RsaSecurityKey LoadRsaSecurityKey(string xml)
    {
        var rsa = RSA.Create();
        rsa.FromXmlString(xml);

        return new RsaSecurityKey(rsa);
    }
    
    public static RsaSecurityKey CreateRsaSecurityKey(int size)
    {
        var rsa = RSA.Create(size);

        return new RsaSecurityKey(rsa);
    }
}