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

//Setup NLog, load configuration from IConfiguration
var nlogBuilder = LogManager.Setup();
nlogBuilder.SetupExtensions(e => e.RegisterNLogWeb())
    .LoadConfigurationFromSection(builder.Configuration);
builder.Host.UseNLog();

var services = builder.Services;

var onAws = builder.Configuration.GetValue<bool>("Nuages:UseAWS");

if (onAws)
{
    //Will Enable Lambda hosting if running in a lambda function, otherwise do nothing.
    services.AddAWSLambdaHosting(LambdaEventSource.RestApi); 
    
    //Save Data Protection key to AWS SM Paramtere Store
    services.AddDataProtection()
        .PersistKeysToAWSSystemsManager("Nuages.Identity.UI/DataProtection"); 
}
else
{
    services.AddDataProtection();
}

services.AddNuagesIdentity(builder.Configuration); 

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
    .AddNuagesLocalization(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

services.AddHttpContextAccessor();

services.AddWebOptimizer(!builder.Environment.IsDevelopment(), !builder.Environment.IsDevelopment());

services.AddHealthChecks();

services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

var corsDomains = builder.Configuration["AllowedCorsDomain"];
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
//app.UseHttpsRedirection();
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
public partial class Program //Require for Integration Tests
#pragma warning restore CA1050
{
}