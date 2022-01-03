
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
using Nuages.Identity.UI.OpenidDict;
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

        services.AddIdentityMongoDbProvider<NuagesApplicationUser, NuagesApplicationRole, string>(identity =>
            {
                identity.Stores.ProtectPersonalData = false;

                identity.Password.RequiredLength = 1;
                identity.Password.RequireDigit = false;
                identity.Password.RequireLowercase = false;
                identity.Password.RequireUppercase = false;
                identity.Password.RequiredUniqueChars = 1;
                identity.Password.RequireNonAlphanumeric = false;
                // other options
            },
            mongo =>
            {
                mongo.ConnectionString =
                    "mongodb+srv://nuages:wCFwlSoX4qK200E1@nuages-dev-2.qxak3.mongodb.net/nuages_identity_2?replicaSet=atlas-jugbu4-shard-0&readPreference=primary&connectTimeoutMS=10000&authSource=admin&authMechanism=SCRAM-SHA-1";

                // other options
            }).AddNuagesIdentity();

        services.AddControllers();

        services.AddRazorPages().AddNuagesLocalization(_configuration);
        services.AddMvc().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }).AddNuagesLocalization(_configuration);

        services.AddHttpContextAccessor();

        var enableOptimizer = !_env.IsDevelopment();
        services.AddWebOptimizer(enableOptimizer, enableOptimizer);

        services.AddScoped<IRecaptchaValidator, GoogleRecaptchaValidator>();

        services.AddAuthentication();

        services.AddGoogleRecaptcha(configure => { configure.Key = "6Ldnbg4aAAAAAOvc8cYjFb2R-tuAEkh_GqHU5AwM"; });
        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
        });

        services.AddOpenIddict()
            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                options.UseMongoDb()
                    .UseDatabase(new MongoClient(_configuration.GetValue<string>("Nuages:Mongo:ConnectionString"))
                        .GetDatabase("openiddict"));
            })

            // Register the OpenIddict server components.
            .AddServer(options =>
            {
#if DEBUG
                options.DisableAccessTokenEncryption();
#endif
                // Enable the authorization, logout, token and userinfo endpoints.
                options.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetLogoutEndpointUris("/connect/logout")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserinfoEndpointUris("/connect/userinfo");

                // Mark the "email", "profile" and "roles" scopes as supported scopes.
                options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles);

                // Note: the sample uses the code and refresh token flows but you can enable
                // the other flows if you need to support implicit, password or client credentials.
                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .AllowPasswordFlow()
                    .AllowClientCredentialsFlow();

                // Register the signing and encryption credentials.
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                // options.AddEphemeralSigningKey();
                // options.AddEphemeralEncryptionKey();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .EnableTokenEndpointPassthrough();
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

#if DEBUG
        services.AddHostedService<OpenIdDictInitializeWorker>();
#endif

        services.AddSingleton<IConfigureOptions<OpenIddictServerOptions>, OpenIddictServerOptionsInitializer>();
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