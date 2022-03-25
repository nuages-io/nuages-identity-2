using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.Fido2;

public interface IFido2SignInManager
{
    Task<SignInResult> SignInAsync();
}