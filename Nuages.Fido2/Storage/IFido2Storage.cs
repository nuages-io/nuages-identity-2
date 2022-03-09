using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Nuages.Fido2.Storage;

public interface IFido2Storage
{
    Task<Fido2User?> GetUserAsync(string userName);
    Task<List<IFido2Credential>> GetCredentialsByUserAsync(Fido2User user);
    Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] argsCredentialId, object cancellationToken);
    Task AddCredentialToUserAsync(Fido2User user, IFido2Credential credential);
    
    void Initialize();


    IFido2Credential CreateCredential(PublicKeyCredentialDescriptor publicKeyCredentialDescriptor, 
        byte[] resultPublicKey, byte[] userId, uint resultCounter, string credType, DateTime now, Guid resultAaguid);
}
