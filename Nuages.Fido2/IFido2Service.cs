using Fido2NetLib;
using Fido2NetLib.Objects;
using Nuages.Fido2.Models;
using Nuages.Fido2.Storage;

namespace Nuages.Fido2;

public interface IFido2Service
{
    Task<CredentialCreateOptions> MakeCredentialOptionsAsync(MakeCredentialOptionsRequest request);
    Task<Fido2NetLib.Fido2.CredentialMakeResult> MakeNewCredentialAsync(AuthenticatorAttestationRawResponse attestationResponse, CancellationToken cancellationToken);
    Task<AssertionOptions> AssertionOptionAsync(AssertionOptionsRequest request);
    Task<AssertionVerificationResult> MakeAssertionAsync(AuthenticatorAssertionRawResponse clientResponse, CancellationToken cancellationToken);

    Task<List<IFido2Credential>> GetSecurityKeysForUser(Fido2User user);
}