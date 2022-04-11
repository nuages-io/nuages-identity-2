using Nuages.AWS.Secrets;
using Nuages.Web;

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

        var configuration = configBuilder.Build();

        var useAws = builder.Configuration.GetValue<bool>("Nuages:UseAWS");
        
        if (!builder.Environment.IsDevelopment() && useAws)
        {
            var config = new ApplicationConfig();
            
            configuration.Bind("Nuages:ApplicationConfig", config);

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
        
        builder.Configuration.TransformSecrets();

        return configBuilder;

    }
}