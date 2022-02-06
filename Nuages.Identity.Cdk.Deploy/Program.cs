using Amazon.CDK;
using Microsoft.Extensions.Configuration;
using Environment = Amazon.CDK.Environment;

// ReSharper disable ArrangeTypeModifiers

// ReSharper disable ObjectCreationAsStatement

namespace Nuages.Identity.Cdk.Deploy;


// ReSharper disable once ClassNeverInstantiated.Global
sealed class Program
{
    // ReSharper disable once UnusedParameter.Global
    public static void Main(string[] args)
    {
        var configManager = new ConfigurationManager();

        var builder = configManager
            .AddJsonFile("appsettings.json",  false, true)
            .AddJsonFile("appsettings.prod.json",  false, true)
            .AddEnvironmentVariables();
        
        var configuration = builder.Build();

        var options = configuration.Get<ConfigOptions>();
            
        var app = new App();
            
            
        var stack = new MyNuagesSenderCdkStack(app, options.StackName, new StackProps
        {
            Env = new Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        });
            
        stack.Node.SetContext("DomainName",  options.DomainName);
        stack.Node.SetContext("DomainNameApi",  options.DomainNameApi);
        stack.Node.SetContext("CertificateArn",  options.CertificateArn);
        
        stack.CreateTemplate();
            
        app.Synth();
    }
}