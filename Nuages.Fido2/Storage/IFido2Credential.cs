using Fido2NetLib.Objects;

namespace Nuages.Fido2.Storage;

#nullable disable

public interface IFido2Credential
{
    public byte[] UserId { get; set; }
    
    public PublicKeyCredentialDescriptor Descriptor { get; set; }

    public byte[] PublicKey { get; set; }
    public byte[] UserHandle { get; set; }
    public uint SignatureCounter { get; set; }
    public string CredType { get; set; }
    public DateTime RegDate { get; set; }
    public Guid AaGuid { get; set; }

    public string DisplayName { get; set; }
}