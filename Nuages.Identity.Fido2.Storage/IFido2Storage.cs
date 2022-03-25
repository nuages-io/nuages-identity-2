using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Nuages.Identity.Fido2.Storage;

public interface IFido2Storage
{
    Task<Fido2User?> GetUserByUsernameAsync(string userName);
    Task<string?> GetUserEmailAsync(byte[] id);
    Task<List<IFido2Credential>> GetCredentialsByUserAsync(Fido2User user);
    Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] argsCredentialIdn);
    Task AddCredentialToUserAsync(Fido2User user, IFido2Credential credential);
    
    void Initialize();


    IFido2Credential CreateCredential(PublicKeyCredentialDescriptor publicKeyCredentialDescriptor, 
        byte[] resultPublicKey, byte[] userId, uint resultCounter, string credType, DateTime now, Guid resultAaguid);

    Task<IFido2Credential?> GetCredentialByIdAsync(byte[] id);
    
    Task<List<IFido2Credential>> GetCredentialsByUserHandleAsync(byte[] argsUserHandl);
    Task UpdateCounterAsync(byte[] resCredentialId, uint resCounter);
    Task RemoveCredentialFromUser(byte[] userId, byte[]  keyId);
}
