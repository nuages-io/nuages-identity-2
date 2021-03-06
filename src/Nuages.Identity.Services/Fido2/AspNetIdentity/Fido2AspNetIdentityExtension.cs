using Fido2NetLib;
using Microsoft.AspNetCore.Identity;
using Nuages.Identity.Services.Fido2.Storage;

namespace Nuages.Identity.Services.Fido2.AspNetIdentity;

public static class Fido2AspNetIdentityExtension
{
    public static Fido2Builder AddNuagesFido2(this IdentityBuilder builder,  Action<Fido2Configuration> setupAction)
    {
        var fido2Builder = new Fido2Builder(builder.Services);
        
        var userType = builder.UserType;
        
        fido2Builder.Services.AddScoped(typeof(IFido2UserStore), typeof(Fido2UserStore<>).MakeGenericType(userType));
        fido2Builder.Services.AddScoped(typeof(IFido2SignInManager), typeof(Fido2SignInManager<>).MakeGenericType(userType));

        fido2Builder.Services.AddNuagesFido2(setupAction);

        builder.AddTokenProvider("FIDO2", typeof(Fifo2UserTwoFactorTokenProvider<>).MakeGenericType(userType));
        
        return fido2Builder;
    }
}