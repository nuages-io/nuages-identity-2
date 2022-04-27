using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nuages.Identity.Services.AspNetIdentity;
using OpenIddict.Server;

namespace Nuages.Identity.UI.OpenIdDict;

public class ServerOptionsInitializer : IConfigureNamedOptions<OpenIddictServerOptions>
{
    private readonly IKeyStore _keyStore;

    public ServerOptionsInitializer(IKeyStore keyStore)
    {
        _keyStore = keyStore;
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
        var key  =_keyStore.GetEncryptionKey();

        if (key == null)
        {
            key = _keyStore.CreateEncryptionKey();
            
            _keyStore.SaveEncryptionKey(key);
        }

        var encryptionCredential = new EncryptingCredentials(key,
            SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512);

        options.EncryptionCredentials.Add(encryptionCredential);
    }

    private void SetupSigningKey(OpenIddictServerOptions options)
    {
        var key  =_keyStore.GetSigningKey();

        if (key == null)
        {
            key = _keyStore.CreateSigningKey();
            
            _keyStore.SaveSigningKey(key);
        }

        var signingCredential = new SigningCredentials(key,
            SecurityAlgorithms.RsaSha256);

        options.SigningCredentials.Add(signingCredential);
    }
  
}