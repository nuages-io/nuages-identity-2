using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Fido2NetLib.Objects;
using Nuages.Identity.Services.Fido2.Storage;

// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.Services.AspNetIdentity;

#nullable disable

public class Fido2Credential : IFido2Credential
{
    public string Id { get; set; }
    public byte[] UserId { get; set; }
    public string UserIdBase64 { get; set; }
    public byte[] PublicKey { get; set; }
    public byte[] UserHandle { get; set; }
    public uint SignatureCounter { get; set; }
    public string CredType { get; set; }
    public DateTime RegDate { get; set; }
    public Guid AaGuid { get; set; }

    public string  DescriptorIdBase64 { get; set; }
    public string DescriptorType { get; set; }
    public string  DescriptorTransports { get; set; }
    
    public string  UserHandleBase64 { get; set; }
    
    public string DisplayName { get; set; }
    
    [NotMapped]
    public PublicKeyCredentialDescriptor Descriptor
    {
        get => string.IsNullOrWhiteSpace(DescriptorJson) ? null : JsonSerializer.Deserialize<PublicKeyCredentialDescriptor>(DescriptorJson);
        set
        {
            DescriptorJson = JsonSerializer.Serialize(value);

            DescriptorIdBase64 = Convert.ToBase64String(value.Id);
            DescriptorType = value.Type?.ToString();
            DescriptorTransports = value.Transports == null ? null : string.Join(",", value.Transports);
        }
    }

    public string DescriptorJson { get; set; }
}