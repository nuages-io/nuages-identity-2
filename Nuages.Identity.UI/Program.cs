using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Amazon;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nuages.AspNetIdentity.Stores.Mongo;
using Nuages.Fido2.AspNetIdentity;
using Nuages.Fido2.Storage.EntifyFramework.MySql;
using Nuages.Fido2.Storage.EntityFramework;
using Nuages.Fido2.Storage.EntityFramework.SqlServer;
using Nuages.Fido2.Storage.Mongo;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Storage.MySql;
using Nuages.Identity.Storage.SqlServer;
using Nuages.Identity.UI;
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
    var config = configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>();

    if (config.ParameterStore.Enabled)
    {
        configBuilder.AddSystemsManager(configureSource =>
        {
            configureSource.Path = config.ParameterStore.Path;
            configureSource.Optional = true;
        });
    }

    if (config.AppConfig.Enabled)
    {
        configBuilder.AddAppConfig(config.AppConfig.ApplicationId,
            config.AppConfig.EnvironmentId,
            config.AppConfig.ConfigProfileId, true);
    }
}

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

var identityBuilder = services.AddNuagesAspNetIdentity<NuagesApplicationUser<string>, NuagesApplicationRole<string>>(
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
    });

var storage = Enum.Parse<StorageType>(configuration["Nuages:Storage"]);

switch (storage)
{
    case StorageType.SqlServer:
    {
        builder.Services.AddDbContext<IdentitySqlServerDbContext>(options =>
        {
            var connectionString = configuration["Nuages:SqlServer:ConnectionString"];

            options
                .UseSqlServer(connectionString);
        });

        identityBuilder.AddEntityFrameworkStores<IdentitySqlServerDbContext>();
        break;
    }
    case StorageType.MySql:
    {
        builder.Services.AddDbContext<IdentityMySqlDbContext>(options =>
        {
            var connectionString = configuration["Nuages:MySql:ConnectionString"];

            options
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });
        identityBuilder.AddEntityFrameworkStores<IdentityMySqlDbContext>();
        break;
    }
    case StorageType.InMemory:
    {
        builder.Services
            .AddDbContext<IdentityDbContext<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>>();
        identityBuilder.AddEntityFrameworkStores<IdentityDbContext<NuagesApplicationUser<string>,NuagesApplicationRole<string>, string>>();

        break;
    }
    case StorageType.MongoDb:
    {
        identityBuilder.AddMongoStores<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>(options =>
        {
            options.ConnectionString = configuration["Nuages:Mongo:ConnectionString"];
            options.Database = configuration["Nuages:Mongo:Database"];

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
        throw new ArgumentOutOfRangeException();
}

identityBuilder.AddNuagesIdentityServices(configuration, _ => { });

var fidoBuilder2 = identityBuilder.AddNuagesFido2(options =>
{
    options.ServerDomain = configuration["fido2:serverDomain"];
    options.ServerName = configuration["fido2:serverName"];
    options.Origins = new HashSet<string> { configuration["fido2:origin"] };
    options.TimestampDriftTolerance = configuration.GetValue<int>("fido2:timestampDriftTolerance");
    options.MDSCacheDirPath = configuration["fido2:MDSCacheDirPath"];
});

switch (storage)
{
    case StorageType.InMemory:
    {
        fidoBuilder2.AddFido2InMemoryStorage("Fido2");
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
        fidoBuilder2.AddFido2MySqlStorage(configuration["Nuages:MySql:ConnectionStringFido2"]);
        break;
    }
    case StorageType.SqlServer:
    {
        fidoBuilder2.AddFidoSqlServerStorage(configuration["Nuages:SqlServer:ConnectionStringFido2"]);
        break;
    }
    default:
        throw new ArgumentOutOfRangeException();
}

services.AddNuagesAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["Google:ClientId"];
        googleOptions.ClientSecret = configuration["Google:ClientSecret"];
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


app.Run();

// ReSharper disable once ClassNeverInstantiated.Global
#pragma warning disable CA1050
public partial class Program
#pragma warning restore CA1050
{
}