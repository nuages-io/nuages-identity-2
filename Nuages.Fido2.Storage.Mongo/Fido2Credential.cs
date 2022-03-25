using Fido2NetLib.Objects;
using MongoDB.Bson.Serialization.Attributes;
using Nuages.Identity.Services.Fido2.Storage;

namespace Nuages.Fido2.Storage.Mongo;

#nullable disable

public class Fido2Credential : IFido2Credential
{
    [BsonId]
    public string Id { get; set; }
    
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