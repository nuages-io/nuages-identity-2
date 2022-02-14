using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Amazon;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.WebEncoders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nuages.AspNetIdentity.Core;
using Nuages.AspNetIdentity.Stores.Mongo;
using Nuages.Identity.Services;
using Nuages.Identity.UI.AWS;
using Nuages.Identity.UI.OpenIdDict;
using Nuages.Localization;
using Nuages.Web;
using OpenIddict.Abstractions;

namespace Nuages.Identity.UI;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<WebEncoderOptions>(options => 
        {
            options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
        });
        
        services.AddScoped<IRuntimeConfiguration, RuntimeConfiguration>();
        
        services.AddDataProtection()
            .PersistKeysToAWSSystemsManager("Nuages.Identity.UI/DataProtection");

        services.AddHttpClient();

        AWSXRayRecorder.InitializeInstance(_configuration);

        ConfigureXRay();

        services.AddNuagesAspNetIdentity<NuagesApplicationUser<string>, NuagesApplicationRole<string>>(
                identity =>
                {
                    identity.User = new UserOptions
                    {
                        RequireUniqueEmail = true /* Not the default*/
                    };

                    identity.ClaimsIdentity = new ClaimsIdentityOptions
                    {
                        RoleClaimType = OpenIddictConstants.Claims.Role,
                        UserNameClaimType = OpenIddictConstants.Claims.Name,
                        UserIdClaimType = OpenIddictConstants.Claims.Subject
                    };

                    identity.SignIn = new SignInOptions
                    {
                        RequireConfirmedEmail = true,
                        RequireConfirmedPhoneNumber = false, //MUST be false
                        RequireConfirmedAccount = false //MUST be false
                    };
                })
            .AddNuagesIdentityServices(_configuration, _ => { })
            .AddMongoStores<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>(options =>
            {
                options.ConnectionString = _configuration["Nuages:Mongo:ConnectionString"];
                options.Database = _configuration["Nuages:Mongo:Database"];

                if (!BsonClassMap.IsClassMapRegistered(typeof(NuagesApplicationUser<string>)))
                    BsonClassMap.RegisterClassMap<NuagesApplicationUser<string>>(cm =>
                    {
                        cm.AutoMap();
                        cm.SetIgnoreExtraElements(true);
                        cm.MapMember(c => c.LastFailedLoginReason)
                            .SetSerializer(new EnumSerializer<FailedLoginReason>(BsonType.String));
                    });
            });


        services.AddNuagesAuthentication()
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = _configuration["Google:ClientId"];
                googleOptions.ClientSecret = _configuration["Google:ClientSecret"];
            });

        // ReSharper disable once UnusedParameter.Local

        services
            .AddMvc()
            .AddJsonOptions(jsonOptions =>
            {
                jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddNuagesLocalization(_configuration)
            ;

        services.AddHttpContextAccessor();

        services.AddWebOptimizer(!_env.IsDevelopment(), !_env.IsDevelopment());

        services.AddUI(_configuration);

        services.AddHealthChecks();


        services.AddScoped<IAudienceValidator, AudienceValidator>();
        services.AddScoped<IAuthorizationCodeFlowHandler, AuthorizationCodeFlowHandler>();
        services.AddScoped<IAuthorizeEndpoint, AuthorizeEndpoint>();
        services.AddScoped<IClientCredentialsFlowHandler, ClientCredentialsFlowHandler>();
        services.AddScoped<IDeviceFlowHandler, DeviceFlowHandler>();
        services.AddScoped<ILogoutEndpoint, LogoutEndpoint>();
        services.AddScoped<IPasswordFlowHandler, PasswordFlowHandler>();
        services.AddScoped<ITokenEndpoint, TokenEndpoint>();

        services.AddScoped<IOpenIddictServerRequestProvider, OpenIddictServerRequestProvider>();

        services.AddNuagesOpenIdDict(_configuration, _ => { });
    }

    [ExcludeFromCodeCoverage]
    private void ConfigureXRay()
    {
        if (!_env.IsDevelopment())
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            AWSXRayRecorder.RegisterLogger(LoggingOptions.Console);
        }
        else
        {
            AWSXRayRecorder.RegisterLogger(LoggingOptions.None);
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        ConfigureErrorPage(app, env);

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
            endpoints.MapDefaultControllerRoute();
            endpoints.MapHealthChecks("health");
        });
    }

    [ExcludeFromCodeCoverage]
    private static void ConfigureErrorPage(IApplicationBuilder app, IHostEnvironment env)
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
    }
}