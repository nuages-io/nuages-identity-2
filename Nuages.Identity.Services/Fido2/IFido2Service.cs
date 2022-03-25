using Fido2NetLib;
using Fido2NetLib.Objects;
using Nuages.Identity.Services.Fido2.Models;
using Nuages.Identity.Services.Fido2.Storage;

namespace Nuages.Identity.Services.Fido2;

public interface IFido2Service
{
    Task<CredentialCreateOptions> MakeCredentialOptionsAsync(MakeCredentialOptionsRequest request);
    Task<Fido2NetLib.Fido2.CredentialMakeResult> MakeNewCredentialAsync(AuthenticatorAttestationRawResponse attestationResponse);
    Task<AssertionOptions> AssertionOptionAsync(AssertionOptionsRequest request);
    Task<AssertionVerificationResult> MakeAssertionAsync(AuthenticatorAssertionRawResponse clientResponse);

    Task<List<IFido2Credential>> GetSecurityKeysForUser(byte[] userId);

    Task<bool> HasSecurityKeys(byte[] userId);
    
    Task RemoveKeyAsync(byte[] userId, byte[]  keyId);
}