
using System.Diagnostics.CodeAnalysis;
using Nuages.Localization.Storage.Config.Sources;

namespace Nuages.Identity.API;

/// <summary>
/// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global

// ReSharper disable once ClassNeverInstantiated.Global
public class LocalEntryPoint
{
    [ExcludeFromCodeCoverage]
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName);
                
                config.AddJsonFile("appsettings.local.json", true, true);
 
                config.AddJsonFileTranslation("/locales/fr-CA.json");
                config.AddJsonFileTranslation("/locales/en-CA.json");
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}