using Fido2NetLib;
using Nuages.Fido2.Models;

namespace Nuages.Fido2;

public interface IFido2Service
{
    Task<CredentialCreateOptions> MakeCredentialOptionsAsync(MakeCredentialOptionsRequest request);
    Task RegisterNewCredentialAsync(RegisterCredentialRequest request);
}