using Fido2NetLib.Objects;

namespace Nuages.Identity.Services.Fido2.Storage;

#nullable disable

public interface IFido2Credential
{
    public byte[] UserId { get;  }
    
    public PublicKeyCredentialDescriptor Descriptor { get;  }

    public byte[] PublicKey { get;  }
    public byte[] UserHandle { get; }
    public uint SignatureCounter { get; set; }
}