
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.Runtime;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nuages.Identity.Services;
using Nuages.Identity.UI.Endpoints.Models;
using Nuages.Identity.UI.Endpoints.OpenIdDict;
using Nuages.Localization;
using Nuages.Web.Recaptcha;
using OpenIddict.Abstractions;
using OpenIddict.Server;

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
            .PersistKeysToAWSSystemsManager($"{_configuration["Nuages:IdentityUI:StackName"]}/DataProtection");

        services.AddHttpClient();

        var awsOptions = _configuration.GetAWSOptions();

        if (!_env.IsDevelopment()) awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();

        services.AddDefaultAWSOptions(awsOptions);

        if (!_env.IsDevelopment())
        {
            AWSXRayRecorder.InitializeInstance(_configuration);
            AWSXRayRecorder.RegisterLogger(LoggingOptions.Console);
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        var options = _configuration.GetSection("Nuages:Identity").Get<NuagesIdentityOptions>();
        
        services.AddIdentityMongoDbProvider<NuagesApplicationUser, NuagesApplicationRole, string>(identity =>
            {
                identity.Lockout = new LockoutOptions
                {
                    AllowedForNewUsers = true, /*SupportsUserLockout*/
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
                    AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider,
                    AuthenticatorIssuer =  "Microsoft.AspNetCore.Identity.UI"
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
                    RequireConfirmedEmail = false,
                    RequireConfirmedPhoneNumber = false,
                    RequireConfirmedAccount = false
                };
                
            },
            mongo =>
            {
                mongo.ConnectionString =
                    _configuration["Nuages:Mongo:ConnectionString"];
            }).AddNuagesIdentity(_configuration);

        services.AddMvc().AddJsonOptions(jsonOptions =>
        {
            jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }).AddNuagesLocalization(_configuration);

        services.AddHttpContextAccessor();

        var enableOptimizer = !_env.IsDevelopment();
        services.AddWebOptimizer(enableOptimizer, enableOptimizer);
        
        services.AddUI(_configuration);
        
        services.AddAuthentication();

        services.AddNuagesOpenIdDict(_configuration["Nuages:Mongo:ConnectionString"], "nuages_identity");
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
            endpoints.MapGet("/",
                async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                }).RequireAuthorization();
        });
    }
}