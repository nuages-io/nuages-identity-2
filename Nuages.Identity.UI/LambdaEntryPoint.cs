using System.Diagnostics.CodeAnalysis;
using NLog.Web;
using Nuages.Localization.Storage.Config.Sources;

namespace Nuages.Identity.UI;

/// <summary>
/// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
/// actual Lambda function entry point. The Lambda handler field should be set to
/// 
/// Nuages.Identity.UI::Nuages.Identity.UI.LambdaEntryPoint::FunctionHandlerAsync
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
    Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
{
    /// <summary>
    /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
    /// needs to be configured in this method using the UseStartup() method.
    /// </summary>
    /// <param name="builder"></param>
    protected override void Init(IWebHostBuilder builder)
    {
        builder
            .UseStartup<Startup>();
    }

    /// <summary>
    /// Use this override to customize the services registered with the IHostBuilder. 
    /// 
    /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
    /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
    /// </summary>
    /// <param name="builder"></param>
    protected override void Init(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName);
            #if !DEBUG
            configBuilder.AddJsonFile("appsettings.prod.json", false, true);
            #endif

            configBuilder.AddJsonFileTranslation("/locales/fr-CA.json");
            configBuilder.AddJsonFileTranslation("/locales/en-CA.json");
            
            var name = Environment.GetEnvironmentVariable("Nuages__Identity__StackName");

            if (name != null)
            {
                configBuilder.AddSystemsManager(configureSource =>
                {
                    // Parameter Store prefix to pull configuration data from.
                    configureSource.Path = $"/{name}";

                    // Reload configuration data every 5 minutes.
                    configureSource.ReloadAfter = TimeSpan.FromMinutes(15);

                    // Configure if the configuration data is optional.
                    configureSource.Optional = true;

                    configureSource.OnLoadException += _ =>
                    {
                        // Add custom error handling. For example, look at the exceptionContext.Exception and decide
                        // whether to ignore the error or tell the provider to attempt to reload.
                    };
                });
            }
        }).UseNLog();
        
    }
}