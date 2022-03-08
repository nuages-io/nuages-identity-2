using Fido2NetLib.Objects;
using Nuages.Fido2.Models;

namespace Nuages.Fido2.Storage.Mongo;

public class Fido2Credential : IFido2Credential
{
    public string Id { get; set; }
    
    public byte[] UserId { get; set; }
    public PublicKeyCredentialDescriptor Descriptor { get; set; }
    public byte[] PublicKey { get; set; }
    public byte[] UserHandle { get; set; }
    public uint SignatureCounter { get; set; }
    public string CredType { get; set; }
    public DateTime RegDate { get; set; }
    public Guid AaGuid { get; set; }
}