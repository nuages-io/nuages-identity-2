using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.Lambda.AspNetCoreServer;
using NLog.Web;
using Nuages.Localization.Storage.Config.Sources;
using Nuages.Web;

namespace Nuages.Identity.UI;

/// <summary>
///     This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the
///     actual Lambda function entry point. The Lambda handler field should be set to
///     Nuages.Identity.UI::Nuages.Identity.UI.LambdaEntryPoint::FunctionHandlerAsync
/// </summary>
[ExcludeFromCodeCoverage]
// ReSharper disable once UnusedType.Global
public class LambdaEntryPoint :

    // The base class must be set to match the AWS service invoking the Lambda function. If not Amazon.Lambda.AspNetCoreServer
    // will fail to convert the incoming request correctly into a valid ASP.NET Core request.
    //
    // API Gateway REST API                         -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    // API Gateway HTTP API payload version 1.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    // API Gateway HTTP API payload version 2.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
    // Application Load Balancer                    -> Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction
    // 
    // Note: When using the AWS::Serverless::Function resource with an event type of "HttpApi" then payload version 2.0
    // will be the default and you must make Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction the base class.
    APIGatewayProxyFunction
{
    /// <summary>
    ///     The builder has configuration, logging and Amazon API Gateway already configured. The startup class
    ///     needs to be configured in this method using the UseStartup() method.
    /// </summary>
    /// <param name="builder"></param>
    protected override void Init(IWebHostBuilder builder)
    {
        builder
            .UseStartup<Startup>();
    }

    /// <summary>
    ///     Use this override to customize the services registered with the IHostBuilder.
    ///     It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
    ///     Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
    /// </summary>
    /// <param name="builder"></param>
    protected override void Init(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName);

            configBuilder.AddJsonFileTranslation("/locales/fr-CA.json");
            configBuilder.AddJsonFileTranslation("/locales/en-CA.json");

            var configuration = configBuilder.Build();
            
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
                    config.AppConfig.ConfigProfileId,true);
            }

        }).UseNLog();
    }
}