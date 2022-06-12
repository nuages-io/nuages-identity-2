using Nuages.Identity.AWS;

namespace Nuages.Identity.UI.Setup;

public static class ConfigExtensions
{
    public static IConfigurationBuilder LoadConfiguration(this WebApplicationBuilder builder)
    {
        var configBuilder = builder.Configuration
            .AddJsonFile("appsettings.json", false, true);

        var fileName = builder.Environment.IsDevelopment() ? "appsettings.local.json" : "appsettings.prod.json";
        
        configBuilder.AddJsonFile(fileName, true, true);

        configBuilder.AddEnvironmentVariables();

        configBuilder.Build();

        //Remove if Not referencing Nuages.Identity.Services.AWS
        configBuilder.LoadAWSConfiguration(builder.Configuration);
        
        return configBuilder;

    }
}