using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login;
using Nuages.Identity.Services.Login.MagicLink;
using Nuages.Identity.Services.Manage;
using Nuages.Identity.Services.Password;
using Nuages.Identity.Services.Register;
using Nuages.Web.Utilities;

namespace Nuages.Identity.Services;

public static class NuagesIdentityConfigExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IdentityBuilder AddNuagesIdentityServices(this IdentityBuilder builder, IConfiguration configuration,
        Action<NuagesIdentityOptions> configure)
    {
        var services = builder.Services;

        services.Configure<NuagesIdentityOptions>(configuration.GetSection("Nuages:Identity"));

        services.Configure(configure);

        var userType = builder.UserType;
        var totpProvider = typeof(MagicLinkLoginProvider<>).MakeGenericType(userType);
        builder.AddTokenProvider("MagicLinkLoginProvider", totpProvider);

        services.AddScoped<IEmailValidator, EmailValidator>();

        //Anonymous
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
        services.AddScoped<IResetPasswordService, ResetPasswordService>();
        services.AddScoped<ISendEmailConfirmationService, SendEmailConfirmationService>();
        services.AddScoped<IConfirmEmailService, ConfirmEmailService>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<IRegisterExternalLoginService, RegisterExternalLoginService>();
        services.AddScoped<IMagicLinkService, MagicLinkService>();
        services.AddScoped<ISMSSendCodeService, SMSSendCodeService>();

        //Manage
        services.AddScoped<IChangeEmailService, ChangeEmailService>();
        services.AddScoped<IChangeUserNameService, ChangeUserNameService>();
        services.AddScoped<IChangePasswordService, ChangePasswordService>();
        services.AddScoped<IChangePhoneNumberService, ChangePhoneNumberService>();
        services.AddScoped<ISendSMSVerificationCodeService, SendSMSVerificationCodeService>();
        services.AddScoped<ISendEmailChangeConfirmationService, SendEmailChangeConfirmationService>();
        services.AddScoped<IMFAService, MFAService>();
        services.AddScoped<IProfileService, ProfileService>();

        services.AddScoped<IIdentityEventBus, IdentityConsoleEventBus>();

        return builder;
    }

    public static AuthenticationBuilder AddNuagesAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CookiePolicyOptions>(options => { options.Secure = CookieSecurePolicy.Always; });

        services.AddSingleton<IKeyStore, KeyStoreFromConfiguration>();
        
        var nuagesIdentityOptions = new NuagesIdentityOptions();
        configuration.GetSection("Nuages:Identity").Bind(nuagesIdentityOptions);
        
        const string scheme = "JwtOrCookie";
        
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IKeyStore>((options, keyStore) => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudiences = nuagesIdentityOptions.Audiences,
                    ValidateIssuer = true,
                    ValidIssuer = nuagesIdentityOptions.Authority,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = keyStore.GetSigningKey()
                };
            });
        
        var builder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = scheme;
                options.DefaultChallengeScheme = scheme;
            })
            .AddCookie(NuagesIdentityConstants.EmailNotVerifiedScheme)
            .AddCookie(NuagesIdentityConstants.ResetPasswordScheme)
            .AddCookie(NuagesIdentityConstants.PasswordExpiredScheme)
            .AddJwtBearer()
            .AddPolicyScheme(scheme, scheme,
                options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authorization = context.Request.Headers[HeaderNames.Authorization];
                        if (authorization.Any() && authorization.Single().StartsWith("Bearer"))
                            return JwtBearerDefaults.AuthenticationScheme;
                        
                        return IdentityConstants.ApplicationScheme;
                    };
                });

        return builder;
    }
}