using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;

namespace Nuages.Identity.UI.OpenIdDict;

public class OpenIddictServerOptionsInitializer : IConfigureNamedOptions<OpenIddictServerOptions>
{
    private readonly OpenIdDictOptions _options;

    public OpenIddictServerOptionsInitializer(IOptions<OpenIdDictOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(string name, OpenIddictServerOptions options)
    {
        Configure(options);
    }

    public void Configure(OpenIddictServerOptions options)
    {
        SetupKeys(options);
    }

    private void SetupKeys(OpenIddictServerOptions options)
    {
        SetupEncryptionKey(options);
        SetupSigningKey(options);
    }

    private void SetupEncryptionKey(OpenIddictServerOptions options)
    {
        RsaSecurityKey? key;

        var xml = _options.EncryptionKey;

        if (!string.IsNullOrEmpty(xml))
        {
            key = CreateRsaSecurityKey(xml);
        }
        else
        {
            key = CreateRsaSecurityKey(2048);
            var newKeyXml = key.Rsa.ToXmlString(true);

            Console.WriteLine(new string('-', 150));
            Console.WriteLine("The following EncryptionKey has been created and should be saved and provided by the configuration system (Nuages:OpenIdDict:EncryptionKey)");
            Console.WriteLine(new string('-', 150));
            Console.WriteLine(newKeyXml);
            Console.WriteLine(new string('-', 150));
        }

        var encryptionCredential = new EncryptingCredentials(key,
            SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512);

        options.EncryptionCredentials.Add(encryptionCredential);
    }

    private void SetupSigningKey(OpenIddictServerOptions options)
    {
        RsaSecurityKey? key;

        var xml = _options.SigningKey;

        if (!string.IsNullOrEmpty(xml))
        {
            key = CreateRsaSecurityKey(xml);
        }
        else
        {
            key = CreateRsaSecurityKey(2048);
            var xmlNewKey = key.Rsa.ToXmlString(true);

            Console.WriteLine(new string('-', 150));
            Console.WriteLine("The following SigningKey has been created and should be saved and provided by the configuration system (Nuages:OpenIdDict:SigningKey)");
            Console.WriteLine(new string('-', 150));
            Console.WriteLine(xmlNewKey);
            Console.WriteLine(new string('-', 150));
        }

        var signingCredential = new SigningCredentials(key,
            SecurityAlgorithms.RsaSha256);

        options.SigningCredentials.Add(signingCredential);
    }

    private static RsaSecurityKey CreateRsaSecurityKey(int size)
    {
        var rsa = RSA.Create(size);

        return new RsaSecurityKey(rsa);
    }

    private static RsaSecurityKey CreateRsaSecurityKey(string xml)
    {
        var rsa = RSA.Create();
        rsa.FromXmlString(xml);

        return new RsaSecurityKey(rsa);
    }
}