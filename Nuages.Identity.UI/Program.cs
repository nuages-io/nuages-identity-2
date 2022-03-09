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
using Nuages.AspNetIdentity.Stores.Mongo;
using Nuages.Fido2;
using Nuages.Fido2.AspNetIdentity;
using Nuages.Fido2.Storage.EntityFramework;
using Nuages.Fido2.Storage.Mongo;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;
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

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
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
    .AddNuagesIdentityServices(configuration, _ => { })
    .AddMongoStores<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>(options =>
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



services.AddNuagesAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["Google:ClientId"];
        googleOptions.ClientSecret = configuration["Google:ClientSecret"];
    });

// ReSharper disable once UnusedParameter.Local

services
    .AddMvc()
    .AddJsonOptions(jsonOptions => { jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter()); })
    .AddNuagesLocalization(configuration);



services.AddNuagesFido2(options =>
    {
        options.ServerDomain = configuration["fido2:serverDomain"];
        options.ServerName = configuration["fido2:serverName"];
        options.Origins = new HashSet<string> { configuration["fido2:origin"] };
        options.TimestampDriftTolerance = configuration.GetValue<int>("fido2:timestampDriftTolerance");
        options.MDSCacheDirPath = configuration["fido2:MDSCacheDirPath"];
    })
    .AddAspNetIdentity<NuagesApplicationUser<string>, string>()
    .AddFido2MongoStorage(config =>
    {
        config.ConnectionString = configuration["Nuages:Mongo:ConnectionString"];
    });
    //.AddFido2InMemoryStorage("Fido2");

services.AddHttpContextAccessor();

services.AddWebOptimizer(!builder.Environment.IsDevelopment(), !builder.Environment.IsDevelopment());

services.AddUI(configuration);

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

services.AddNuagesOpenIdDict(configuration, _ => { });

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