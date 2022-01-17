using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.Login.Passwordless;

public static class PasswordlessConfigExtension
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IdentityBuilder AddPasswordlessLoginProvider(this IdentityBuilder builder)
    {
        var userType = builder.UserType;
        var totpProvider = typeof(PasswordlessLoginProvider<>).MakeGenericType(userType);
        return builder.AddTokenProvider("PasswordlessLoginProvider", totpProvider);
    }
}