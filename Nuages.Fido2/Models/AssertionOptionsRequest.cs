using Fido2NetLib.Objects;

namespace Nuages.Fido2.Models;

public class AssertionOptionsRequest
{
    public string UserName { get; set; } = string.Empty;
    public UserVerificationRequirement? UserVerification { get; set; }
}