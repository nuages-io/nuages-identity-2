using Microsoft.IdentityModel.Tokens;
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.OpenIdDict;

public interface IKeyStore
{
    public RsaSecurityKey? GetSigningKey();
    public RsaSecurityKey? GetEncryptionKey();
    
    public RsaSecurityKey CreateSigningKey();
    public RsaSecurityKey CreateEncryptionKey();

    public void SaveSigningKey(RsaSecurityKey key);
    public void SaveEncryptionKey(RsaSecurityKey key);
}