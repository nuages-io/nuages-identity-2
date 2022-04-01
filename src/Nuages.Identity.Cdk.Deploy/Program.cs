using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Microsoft.Extensions.Configuration;
using Nuages.AWS.Secrets;
using Nuages.Web;

// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ObjectCreationAsStatement

namespace Nuages.Identity.Cdk.Deploy;

[ExcludeFromCodeCoverage]
// ReSharper disable once ClassNeverInstantiated.Global
sealed class Program
{
    // ReSharper disable once UnusedParameter.Global
    public static void Main(string[] args)
    {
        var configManager = new ConfigurationManager();

        var builder = configManager
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        var config = configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>();
        
        if (config.ParameterStore.Enabled)
        {
            builder.AddSystemsManager(configureSource =>
            {
                configureSource.Path = config.ParameterStore.Path;
                configureSource.Optional = true;
            });
        }

        if (config.AppConfig.Enabled)
        {
            builder.AddAppConfig(config.AppConfig.ApplicationId,  
                config.AppConfig.EnvironmentId, 
                config.AppConfig.ConfigProfileId,true);
        }
        
        var secretProvider = new AWSSecretProvider();
        secretProvider.TransformSecrets(configManager);
        
        var app = new App();

        if (args.Contains("--pipeline"))
        {
            IdentityStackWithPipeline.Create(app, configuration);
        }
        else
        {
            IdentityStack.CreateStack(app, configuration);
        }

        app.Synth();
    }
}