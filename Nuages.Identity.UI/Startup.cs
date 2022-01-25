using System.Security.Claims;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.Runtime;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Identity;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Login.Passwordless;
using Nuages.Identity.UI.Endpoints.OpenIdDict;
using Nuages.Localization;
using OpenIddict.Abstractions;

namespace Nuages.Identity.UI;

public class Startup
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDataProtection()
            .PersistKeysToAWSSystemsManager("Nuages.Identity.UI/DataProtection");

        services.AddHttpClient();

        AWSXRayRecorder.InitializeInstance(_configuration);
        
        if (!_env.IsDevelopment())
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            AWSXRayRecorder.RegisterLogger(LoggingOptions.Console);
        }
        else
        {
            AWSXRayRecorder.RegisterLogger(LoggingOptions.None);
        }
        
        var options = _configuration.GetSection("Nuages:Identity").Get<NuagesIdentityOptions>();
        
        services.AddIdentityMongoDbProvider<NuagesApplicationUser, NuagesApplicationRole, string>(identity =>
            {
                identity.Lockout = new LockoutOptions
                {
                    AllowedForNewUsers = options.EnableUserLockout,
                    MaxFailedAccessAttempts = 5,
                    DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5)
                };

                identity.Stores = new StoreOptions
                {
                    MaxLengthForKeys = 0,
                    ProtectPersonalData = false
                };

                identity.Tokens = new TokenOptions
                {
                    ProviderMap = new Dictionary<string, TokenProviderDescriptor>(),
                    EmailConfirmationTokenProvider = TokenOptions.DefaultProvider,
                    PasswordResetTokenProvider = TokenOptions.DefaultProvider,
                    ChangeEmailTokenProvider = TokenOptions.DefaultProvider,
                    ChangePhoneNumberTokenProvider = TokenOptions.DefaultPhoneProvider,
                    AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider
                };

                identity.Password = options.Password;

                identity.User = new UserOptions
                {
                    AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+",
                    RequireUniqueEmail = true /* Not the default*/
                };

                identity.ClaimsIdentity = new ClaimsIdentityOptions
                {
                    RoleClaimType = OpenIddictConstants.Claims.Role,
                    UserNameClaimType = OpenIddictConstants.Claims.Name,
                    UserIdClaimType = OpenIddictConstants.Claims.Subject,
                    EmailClaimType = ClaimTypes.Email,
                    SecurityStampClaimType =  "AspNet.Identity.SecurityStamp"
                };

                identity.SignIn = new SignInOptions
                {
                    RequireConfirmedEmail = options.RequireConfirmedEmail,
                    RequireConfirmedPhoneNumber = false, //MUST be false
                    RequireConfirmedAccount = false //MUST be false
                };
                
            },
            mongo =>
            {
                mongo.ConnectionString =
                    _configuration["Nuages:Mongo:ConnectionString"];
            })
            .AddNuagesIdentity(_configuration)
            .AddPasswordlessLoginProvider();

        services.AddMvc().AddJsonOptions(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }).AddNuagesLocalization(_configuration);

        services.AddHttpContextAccessor();

        var enableOptimizer = !_env.IsDevelopment();
        services.AddWebOptimizer(enableOptimizer, enableOptimizer);
        
        services.AddUI(_configuration);

        services.AddAuthentication()
            .AddCookie(NuagesIdentityConstants.EmailNotVerifiedScheme)
            .AddCookie(NuagesIdentityConstants.ResetPasswordScheme)
            .AddCookie(NuagesIdentityConstants.PasswordExpiredScheme)
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = _configuration["Nuages:UI:ExternalLogin:Google:ClientId"];
                googleOptions.ClientSecret = _configuration["Nuages:UI:ExternalLogin:Google:ClientSecret"];
            });

       // var openIdDictOptions = _configuration.GetSection("Nuages:OpenIdDict").Get<OpenIdDictOptions>();
            
       // ReSharper disable once UnusedParameter.Local
       services.AddNuagesOpenIdDict(_configuration, configure => {
            
        });

        services.AddHealthChecks();
        
        
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            //app.UseDeveloperExceptionPage();
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseWebOptimizer();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestLocalization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapHealthChecks("health");
        });
    }
}