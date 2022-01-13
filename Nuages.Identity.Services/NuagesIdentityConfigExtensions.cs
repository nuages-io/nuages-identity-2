
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Sender.API.Sdk;

namespace Nuages.Identity.Services;

public static class NuagesIdentityConfigExtensions
{
    public static void AddNuagesIdentity(this IdentityBuilder builder,
        Action<NuagesIdentityOptions> configure )
    {
        AddNuagesIdentity(builder, null, configure);
    }
    
    public static void AddNuagesIdentity(this IdentityBuilder builder, IConfiguration? configuration = null, Action<NuagesIdentityOptions>? configure = null)
    {
        var services = builder.Services;
        
        if (configuration != null)
        {
            services.Configure<NuagesIdentityOptions>(configuration.GetSection("Nuages:Identity"));
        }
        
        services.AddSenderClient(configuration);
        
        if (configure != null)
            services.Configure(configure);
        
        services.AddScoped(typeof(NuagesUserManager));
        services.AddScoped(typeof(NuagesSignInManager));

        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
        services.AddScoped<IResetPasswordService, ResetPasswordService>();
        services.AddScoped<ILookupProtector, LookupProtector>();
        services.AddScoped<ILookupProtectorKeyRing, LookupProtectorKeyRing>();
        services.AddScoped<ISendEmailConfirmationService, SendEmailConfirmationService>();
        services.AddScoped<IConfirmEmailService, ConfirmEmailService>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<IRegisterExternalLoginService, RegisterExternalLoginService>();
    }
}