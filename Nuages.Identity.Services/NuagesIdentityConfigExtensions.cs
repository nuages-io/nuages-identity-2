using Microsoft.AspNetCore.Identity;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login;
using Nuages.Identity.Services.Login.Passwordless;
using Nuages.Identity.Services.Manage;
using Nuages.Identity.Services.Manage.Admin;
using Nuages.Identity.Services.Password;
using Nuages.Identity.Services.Register;
using Nuages.Sender.API.Sdk;

namespace Nuages.Identity.Services;

public static class NuagesIdentityConfigExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddNuagesIdentity(this IdentityBuilder builder,
        Action<NuagesIdentityOptions> configure )
    {
        AddNuagesIdentity(builder, null, configure);
    }
    
    public static IdentityBuilder AddNuagesIdentity(this IdentityBuilder builder, IConfiguration? configuration = null, Action<NuagesIdentityOptions>? configure = null)
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
        
        services.AddScoped<ISetPasswordService, SetPasswordService>();
        services.AddScoped<IChangeEmailService, ChangeEmailService>();
        services.AddScoped<IChangeUserNameService, ChangeUserNameService>();
        services.AddScoped<IChangePasswordService, ChangePasswordService>();
        services.AddScoped<IChangePhoneNumberService, ChangePhoneNumberService>();

        services.AddScoped<IMFAService, MFAService>();
        services.AddScoped<IPasswordlessService, PasswordlessService>();
        services.AddScoped<ISMSLoginService, SMSLoginService>();
        
        services.AddScoped<ICurrentBaseUrlProvider, CurrentBaseUrlProvider>();
        
        services.AddScoped<IAdminSetPasswordService, AdminSetPasswordService>();
        return builder;
    }
}