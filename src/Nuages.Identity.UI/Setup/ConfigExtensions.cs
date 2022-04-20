using Nuages.Identity.AWS;

namespace Nuages.Identity.UI.Setup;

public static class ConfigExtensions
{
    public static IConfigurationBuilder LoadConfiguration(this WebApplicationBuilder builder)
    {
        var configBuilder = builder.Configuration
            .AddJsonFile("appsettings.json", false, true);

        if (builder.Environment.IsDevelopment())
        {
            configBuilder.AddJsonFile("appsettings.local.json", false, true);
        }
        else
        {
            configBuilder.AddJsonFile("appsettings.prod.json", true, true);
        }

        configBuilder.AddEnvironmentVariables();

        configBuilder.Build();

        //Remove if Not referencing Nuages.Identity.Services.AWS
        configBuilder.LoadAWSConfiguration(builder.Configuration);
        
        return configBuilder;

    }
}