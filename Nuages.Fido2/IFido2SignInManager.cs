using Microsoft.AspNetCore.Identity;

namespace Nuages.Fido2;

public interface IFido2SignInManager
{
    Task<SignInResult> SignInAsync();
}