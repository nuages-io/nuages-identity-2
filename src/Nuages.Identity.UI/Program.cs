using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Amazon;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nuages.AspNetIdentity.Stores.Mongo;
using Nuages.AWS.Secrets;
using Nuages.Fido2.Storage.Mongo;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Identity.Services.Fido2.AspNetIdentity;
using Nuages.Identity.Services.Fido2.Storage;
using Nuages.Identity.UI;
using Nuages.Identity.UI.AWS;
using Nuages.Identity.UI.OpenIdDict;
using Nuages.Localization;
using Nuages.Localization.Storage.Config.Sources;
using Nuages.Web;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var configBuilder = builder.Configuration
    .AddJsonFile("appsettings.json", false, true);

if (builder.Environment.IsDevelopment())
{
    configBuilder.AddJsonFile("appsettings.local.json", false, true);
}

configBuilder.AddEnvironmentVariables();

configBuilder.SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName);

configBuilder.AddJsonFileTranslation("/locales/fr-CA.json");
configBuilder.AddJsonFileTranslation("/locales/en-CA.json");

var configuration = configBuilder.Build();

if (!builder.Environment.IsDevelopment())
{
    var config = configuration.GetSection("Nuages:ApplicationConfig").Get<ApplicationConfig>();

    if (config.ParameterStore.Enabled)
    {
        configBuilder.AddSystemsManager(configureSource =>
        {
            configureSource.Path = config.ParameterStore.Path;
            configureSource.Optional = true;
            configureSource.ReloadAfter = TimeSpan.FromMinutes(15);
        });
    }

    if (config.AppConfig.Enabled)
    {
        configBuilder.AddAppConfig(config.AppConfig.ApplicationId,
            config.AppConfig.EnvironmentId,
            config.AppConfig.ConfigProfileId, true, TimeSpan.FromMinutes(15));
    }
}

var secretProvider = new AWSSecretProvider();
secretProvider.TransformSecrets(builder.Configuration);

var services = builder.Services;

services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

services.AddScoped<IRuntimeConfiguration, RuntimeConfiguration>();

services.AddDataProtection()
    .PersistKeysToAWSSystemsManager("Nuages.Identity.UI/DataProtection");

services.AddHttpClient();

AWSXRayRecorder.InitializeInstance(configuration);

if (!builder.Environment.IsDevelopment())
{
    AWSSDKHandler.RegisterXRayForAllServices();
    AWSXRayRecorder.RegisterLogger(LoggingOptions.Console);
}
else
{
    AWSSDKHandler.RegisterXRayForAllServices();
    AWSXRayRecorder.RegisterLogger(LoggingOptions.None);
}

var identityBuilder = services.AddNuagesAspNetIdentity<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>(
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

        configuration.GetSection("Nuages:Identity:Password").Bind(identity.Password);
        
    });

var storage = Enum.Parse<StorageType>(configuration["Nuages:Storage"]);

switch (storage)
{
    case StorageType.SqlServer:
    {
        builder.Services.AddDbContext<NuagesIdentityDbContext>(options =>
        {
            var connectionString = configuration["Nuages:SqlServer:ConnectionString"];

            options
                .UseSqlServer(connectionString);

            options.UseOpenIddict();
        });

        identityBuilder.AddEntityFrameworkStores<NuagesIdentityDbContext>();
        
        break;
    }
    case StorageType.MySql:
    {
        builder.Services.AddDbContext<NuagesIdentityDbContext>(options =>
        {
            var connectionString = configuration["Nuages:MySql:ConnectionString"];

            options
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            
            options.UseOpenIddict();
        });
        identityBuilder.AddEntityFrameworkStores<NuagesIdentityDbContext>();
        
        
        break;
    }
    case StorageType.InMemory:
    {
        builder.Services
            .AddDbContext<NuagesIdentityDbContext>(options =>
            {
                options.UseInMemoryDatabase("Identity");
                
                options.UseOpenIddict();
            });
        
        identityBuilder.AddEntityFrameworkStores<NuagesIdentityDbContext>();

        break;
    }
    case StorageType.MongoDb:
    {
        identityBuilder.AddMongoStores<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>(options =>
        {
            options.ConnectionString = configuration["Nuages:Mongo:ConnectionString"];

            if (!BsonClassMap.IsClassMapRegistered(typeof(NuagesApplicationUser<string>)))
                BsonClassMap.RegisterClassMap<NuagesApplicationUser<string>>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                    cm.MapMember(c => c.LastFailedLoginReason)
                        .SetSerializer(new EnumSerializer<FailedLoginReason>(BsonType.String));
                });
        });
        break;
    }
    default:
        throw new Exception("Invalid storage");
}

identityBuilder.AddNuagesIdentityServices(configuration, _ => { });

var uri = new Uri(configuration["Nuages:Identity:Authority"]);

var fidoBuilder2 = identityBuilder.AddNuagesFido2(options =>
{
    options.ServerDomain = uri.Host;
    options.ServerName = configuration["Nuages:Identity:Name"];
    options.Origins = new HashSet<string> { configuration["Nuages:Identity:Authority"] };
    options.TimestampDriftTolerance = 300000;
});

identityBuilder.AddMessageService(configure =>
{
    configure.SendFromEmail = configuration["Nuages:MessageService:SendFromEmail"];
    configure.DefaultCulture = configuration["Nuages:MessageService:DefaultCulture"];
});

services.AddAWSSender();

switch (storage)
{
    case StorageType.InMemory:
    {
        builder.Services.AddScoped<IFido2Storage, Fido2StorageEntityFramework<NuagesIdentityDbContext>>();
        break;
    }
    case StorageType.MongoDb:
    {
        fidoBuilder2.AddFido2MongoStorage(config =>
        {
            config.ConnectionString = configuration["Nuages:Mongo:ConnectionString"];
        });
        break;
    }
    case StorageType.MySql:
    {
        builder.Services.AddScoped<IFido2Storage, Fido2StorageEntityFramework<NuagesIdentityDbContext>>();
        break;
    }
    case StorageType.SqlServer:
    {
        builder.Services.AddScoped<IFido2Storage, Fido2StorageEntityFramework<NuagesIdentityDbContext>>();
        break;
    }
    default:
        throw new Exception("Invalid storage");
}

services.AddNuagesAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["OpenIdProviders:Google:ClientId"];
        googleOptions.ClientSecret = configuration["OpenIdProviders:Google:ClientSecret"];
    });

services
    .AddMvc()
    .AddMvcOptions(options => { options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); })
    .AddRazorPagesOptions(options =>
    {
        options.Conventions
            .ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
    })
    .AddJsonOptions(jsonOptions =>
    {
        jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
    })
    .AddNuagesLocalization(configuration);


services.AddHttpContextAccessor();

services.AddWebOptimizer(!builder.Environment.IsDevelopment(), !builder.Environment.IsDevelopment());

services.AddUI(configuration);

services.AddHealthChecks();

services.AddNuagesOpenIdDict(configuration, _ => { });

services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

var corsDomains = configuration["AllowedCorsDomain"];
if (!string.IsNullOrEmpty(corsDomains))
    services.AddCors( corsDomains.Split(",") );

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseWebOptimizer();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy();
    
app.UseRouting();

app.UseCors(CorsConfigExtensions.AllowSpecificOrigins);

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

app.Run();

// ReSharper disable once ClassNeverInstantiated.Global
#pragma warning disable CA1050
public partial class Program
#pragma warning restore CA1050
{
}