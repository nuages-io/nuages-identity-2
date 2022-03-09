using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Fido2NetLib.Objects;

namespace Nuages.Fido2.Storage.EntityFramework;

#nullable disable

public class Fido2Credential : IFido2Credential
{
    public string Id { get; set; }
    public string Username { get; set; }
    public byte[] UserId { get; set; }
    public string UserIdBase64 { get; set; }
    public byte[] PublicKey { get; set; }
    public byte[] UserHandle { get; set; }
    public uint SignatureCounter { get; set; }
    public string CredType { get; set; }
    public DateTime RegDate { get; set; }
    public Guid AaGuid { get; set; }

    [NotMapped]
    public PublicKeyCredentialDescriptor Descriptor
    {
        get { return string.IsNullOrWhiteSpace(DescriptorJson) ? null : JsonSerializer.Deserialize<PublicKeyCredentialDescriptor>(DescriptorJson); }
        set => DescriptorJson = JsonSerializer.Serialize(value);
    }
    public string DescriptorJson { get; set; }
}