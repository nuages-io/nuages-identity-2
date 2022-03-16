using Fido2NetLib;

namespace Nuages.Fido2.Storage;

public interface IFido2UserStore
{
    Task<Fido2User?> GetUserAsync(string userName);
}