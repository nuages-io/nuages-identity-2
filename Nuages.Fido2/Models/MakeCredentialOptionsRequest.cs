using Fido2NetLib.Objects;

namespace Nuages.Fido2.Models;

public class MakeCredentialOptionsRequest
{
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AttestationConveyancePreference AttestationType { get; set; }
    public AuthenticatorAttachment? AuthType { get; set; }
    public bool RequireResidentKey { get; set; }
    public UserVerificationRequirement UserVerification { get; set; }
}
