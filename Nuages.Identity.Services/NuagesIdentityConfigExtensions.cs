using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.AspNetIdentity.Mongo;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login;
using Nuages.Identity.Services.Login.Passwordless;
using Nuages.Identity.Services.Manage;
using Nuages.Identity.Services.OpenIdDict;
using Nuages.Identity.Services.Password;
using Nuages.Identity.Services.Register;
using Nuages.Sender.API.Sdk;
using Nuages.Web.Utilities;

namespace Nuages.Identity.Services;

[ExcludeFromCodeCoverage]
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
        
        builder.AddUserManager<NuagesUserManager>()
            .AddSignInManager<NuagesSignInManager>()
            .AddRoleManager<NuagesRoleManager>();
        
        BsonClassMap.RegisterClassMap<MongoIdentityUserToken<string>>(cm => 
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id);
            cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
            
        });
        
        BsonClassMap.RegisterClassMap<IdentityUserToken<string>>(cm => 
        {
            cm.AutoMap();
            cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });
        
        BsonClassMap.RegisterClassMap<IdentityRole<string>>(cm => 
        {
            cm.AutoMap();
            cm.MapMember(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        
        BsonClassMap.RegisterClassMap<IdentityUser<string>>(cm => 
        {
            cm.AutoMap();
            cm.MapMember(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        
        BsonClassMap.RegisterClassMap<NuagesApplicationUser>(cm => 
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.MapMember(c => c.LastFailedLoginReason).SetSerializer(new EnumSerializer< FailedLoginReason>(BsonType.String));

        });
        
        builder.AddUserStore<MongoUserStore<NuagesApplicationUser, NuagesApplicationRole, string>>();
        builder.AddRoleStore<MongoRoleStore<NuagesApplicationRole, string>>();
        
        services.AddScoped<IEmailValidator, EmailValidator>();
        
        //Anonymous
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
        services.AddScoped<IResetPasswordService, ResetPasswordService>();
        services.AddScoped<ISendEmailConfirmationService, SendEmailConfirmationService>();
        services.AddScoped<IConfirmEmailService, ConfirmEmailService>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<IRegisterExternalLoginService, RegisterExternalLoginService>();
        services.AddScoped<IPasswordlessService, PasswordlessService>();
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
        
        services.AddScoped<IMessageService, MessageService>();
        
        services.AddScoped<IAudienceValidator, AudienceValidator>();
        services.AddScoped<IAuthorizationCodeFlowHandler, AuthorizationCodeFlowHandler>();
        services.AddScoped<IAuthorizeEndpoint, AuthorizeEndpoint>();
        services.AddScoped<IClientCredentialsFlowHandler, ClientCredentialsFlowHandler>();
        services.AddScoped<IDeviceFlowHandler, DeviceFlowHandler>();
        services.AddScoped<ILogoutEndpoint, LogoutEndpoint>();
        services.AddScoped<IPasswordFlowHandler, PasswordFlowHandler>();
        services.AddScoped<ITokenEndpoint, TokenEndpoint>();
        
        return builder;
    }
}