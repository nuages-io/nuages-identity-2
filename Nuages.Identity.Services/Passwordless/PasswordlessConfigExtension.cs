using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.Passwordless;

public static class PasswordlessConfigExtension
{
    public static IdentityBuilder AddPasswordlessLoginProvider(this IdentityBuilder builder)
    {
        var userType = builder.UserType;
        var totpProvider = typeof(PasswordlessLoginProvider<>).MakeGenericType(userType);
        return builder.AddTokenProvider("PasswordlessLoginProvider", totpProvider);
    }
}