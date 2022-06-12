using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.WebEncoders;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using Nuages.Identity.UI.Setup;
using Nuages.Localization;
using Nuages.Localization.Storage.Config.Sources;
using Nuages.Web;

var builder = WebApplication.CreateBuilder(args);

var configBuilder = builder.LoadConfiguration();

//Load Language files
configBuilder.SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName);
configBuilder.AddJsonFileTranslation("/locales/fr-CA.json");
configBuilder.AddJsonFileTranslation("/locales/en-CA.json");

var config = builder.Configuration;

//Setup NLog, load configuration from IConfiguration

LogManager.Setup().SetupExtensions(e => e.RegisterNLogWeb())
    .LoadConfigurationFromSection(config);

builder.Host.UseNLog();

var services = builder.Services;

services.AddDataProtection();



var redis = config["Nuages:Data:Redis"];
if (!string.IsNullOrEmpty(redis))
{
    Console.WriteLine("Using REDIS as Cache");
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redis;
        options.InstanceName = "Nuages";
    });
}
else
{
    //Default distributed memory cache, you might want to use something else when running in a web farm
    //https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-6.0
    services.AddDistributedMemoryCache();
}



services.AddNuagesIdentity(config); 

//Do not remove if you use accentuated language
services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

//Helper for Tests
services.AddScoped<IRuntimeConfiguration, RuntimeConfiguration>();

services.AddHttpClient();

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
    .AddNuagesLocalization(config);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

services.AddHttpContextAccessor();

services.AddWebOptimizer(!builder.Environment.IsDevelopment(), !builder.Environment.IsDevelopment());

builder.Services.AddSwaggerDocument(configSwagger =>
{
    configSwagger.PostProcess = document =>
    {
        document.Info.Version = "v1";
        document.Info.Title = "Nuages Identity";

        document.Info.Contact = new NSwag.OpenApiContact
        {
            Name = "Nuages.io",
            Email = "martin@nuages.io",
            Url = "https://github.com/nuages-io/nuages-identity-2"
        };
        document.Info.License = new NSwag.OpenApiLicense
        {
            Name = "Use under LICENCE",
            Url = "http://www.apache.org/licenses/LICENSE-2.0"
        };
    };
});


services.AddHealthChecks();

services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

var corsDomains = config["Nuages:AllowedCorsDomain"];
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
    app.UseForwardedHeaders();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseWebOptimizer();

app.UseHttpsRedirection();

app.UseStaticFiles();

if (UseCookiePolicy)
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

app.UseOpenApi();
app.UseSwaggerUi3();

app.Run();

// ReSharper disable once ClassNeverInstantiated.Global
#pragma warning disable CA1050
[ExcludeFromCodeCoverage]
public partial class Program //Require for Integration Tests
#pragma warning restore CA1050
{ 
    public static bool UseCookiePolicy = true;
}