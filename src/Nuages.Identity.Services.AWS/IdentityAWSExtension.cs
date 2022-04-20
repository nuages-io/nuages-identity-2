// ReSharper disable InconsistentNaming

using Nuages.AWS.Secrets;
using Nuages.Identity.AWS.Sender;
using Nuages.Web;

namespace Nuages.Identity.AWS;

public static class IdentityAWSExtension
{
    public static void AddAWS(this IServiceCollection services, IConfiguration configuration, string templateFileName, bool initializeTemplate = false)
    {
        if (configuration.GetValue<bool>("Nuages:UseAWS"))
        {
            //Add options from Configuration if available
            //https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
    
            //Save Data Protection key to AWS SM Paramtere Store
            services.AddDataProtection()
                .PersistKeysToAWSSystemsManager("Nuages.Identity.UI/DataProtection");
        
            services.AddAWSSender(templateFileName, initializeTemplate);
        }
    }

    public static void LoadAWSConfiguration(this IConfigurationBuilder configBuilder, ConfigurationManager configManager)
    {
        if (configManager.GetValue<bool>("Nuages:UseAWS"))
        {
            var config = new ApplicationConfig();
            
            configManager.Bind("Nuages:ApplicationConfig", config);

            if (config.ParameterStore.Enabled)
            {
                configBuilder.AddSystemsManager(configureSource =>
                {
                    configureSource.Path = config.ParameterStore.Path;
                    configureSource.Optional = true;
                    configureSource.ReloadAfter = TimeSpan.FromMinutes(15);
                });
            }

            Console.WriteLine($"config.AppConfig.Enabled = {config.AppConfig.Enabled}");
            
            if (config.AppConfig.Enabled)
            {
                configBuilder.AddAppConfig(config.AppConfig.ApplicationId,
                    config.AppConfig.EnvironmentId,
                    config.AppConfig.ConfigProfileId, true, TimeSpan.FromMinutes(15));
            }
            
            configManager.TransformSecrets();
        }
    }
}