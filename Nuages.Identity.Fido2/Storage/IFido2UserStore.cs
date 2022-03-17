using Fido2NetLib;

namespace Nuages.Fido2.Storage;

public interface IFido2UserStore
{
    Task<Fido2User?> GetUserByUsernameAsync(string userName);
    Task<string?> GetUserEmailAsync(byte[] id);
}