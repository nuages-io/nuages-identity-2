// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using Amazon.EventBridge;
using Amazon.Runtime;
using Nuages.AWS.Secrets;
using Nuages.Identity.AWS.Sender;
using Nuages.Identity.Services;
using Nuages.Web;

namespace Nuages.Identity.AWS;

public static class IdentityAWSExtension
{
    public static void AddAWS(this IServiceCollection services, IConfiguration configuration, string templateFileName, bool initializeTemplate = false)
    {
        if (configuration.GetValue<bool>("Nuages:UseAWS"))
        {
            Console.WriteLine("AWS Host is Enabled");

            var awsOption = configuration.GetAWSOptions();
            
            //Add options from Configuration if available
            //https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html
           
            var environmentVariable = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            if (!string.IsNullOrEmpty(environmentVariable))
                awsOption.Credentials = new EnvironmentVariablesAWSCredentials();
            
            services.AddDefaultAWSOptions(awsOption);
            
            //Save Data Protection key to AWS SM Paramtere Store
            services.AddDataProtection()
                .PersistKeysToAWSSystemsManager("Nuages.Identity.UI/DataProtection");
        
            services.AddAWSSender(templateFileName, initializeTemplate);
            
            services.AddEventBus(configure =>
            {
                configure.Name = configuration["Nuages:EventBus:Name"];
                configure.Source = configuration["Nuages:EventBus:Source"];
            });
            
        }
        else
        {
            Console.WriteLine("AWS Host is Disabled");
        }
    }

    static void AddEventBus(this IServiceCollection services, Action<EventBusOptions> configure)
    {
        services.Configure(configure);
        services.AddScoped<IIdentityEventBus, AwsIdentityEventBus>();
        services.AddAWSService<IAmazonEventBridge>();
    }
    
    public class EventBusOptions
    {
        public string? Name { get; set; }
        public string? Source { get; set; }    
    }
    
    public static void LoadAWSConfiguration(this IConfigurationBuilder configBuilder, ConfigurationManager configManager)
    {
        Console.WriteLine("LoadAWSConfiguration");
        
        if (configManager.GetValue<bool>("Nuages:UseAWS"))
        {
            var config = new ApplicationConfig();
            
            configManager.Bind("Nuages:ApplicationConfig", config);

            LoadParameterStoreConfiguration(configBuilder, config);

            Console.WriteLine($"config.AppConfig.Enabled = {config.AppConfig.Enabled}");
            
            LoadAppConfigConfiguration(configBuilder, config);

            configManager.TransformSecrets();
        }
    }

    [ExcludeFromCodeCoverage]
    private static void LoadAppConfigConfiguration(IConfigurationBuilder configBuilder, ApplicationConfig config)
    {
        Console.WriteLine("LoadAppConfigConfiguration");
        
        if (config.AppConfig.Enabled)
        {
            Console.WriteLine($"config.AppConfig.ApplicationId = {config.AppConfig.ApplicationId}");
            Console.WriteLine($"config.AppConfig.EnvironmentId = {config.AppConfig.EnvironmentId}");
            Console.WriteLine($"config.AppConfig.ConfigProfileId = {config.AppConfig.ConfigProfileId}");
            
            configBuilder.AddAppConfig(config.AppConfig.ApplicationId,
                config.AppConfig.EnvironmentId,
                config.AppConfig.ConfigProfileId, true, TimeSpan.FromMinutes(15));
        }
    }

    [ExcludeFromCodeCoverage]
    private static void LoadParameterStoreConfiguration(IConfigurationBuilder configBuilder, ApplicationConfig config)
    {
        if (config.ParameterStore.Enabled)
        {
            configBuilder.AddSystemsManager(configureSource =>
            {
                configureSource.Path = config.ParameterStore.Path;
                configureSource.Optional = true;
                configureSource.ReloadAfter = TimeSpan.FromMinutes(15);
            });
        }
    }
}