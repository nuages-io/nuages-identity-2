using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.CDK;
using Constructs;
using Microsoft.Extensions.Configuration;
using Nuages.Identity.CDK;

namespace Nuages.Identity.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class IdentityStack : IdentityCdkStack
{
    private IdentityStack(Construct scope, string id, IStackProps props) : base(scope, id, props)
    {
        AssetUi = "./src/Nuages.Identity.UI/bin/Release/net6.0/linux-x64/publish";
    }
    
    public static void CreateStack(Construct scope, IConfiguration configuration)
    {
        var options = configuration.Get<ConfigOptions>();

        Console.WriteLine(JsonSerializer.Serialize(options));
        
        var stack = new IdentityStack(scope, "Stack", new StackProps
        {
            StackName = options.StackName,
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        })
        {
            DomainName = options.DomainName,
            CertificateArn = options.CertificateArn
        };
        
        stack.CreateTemplate();
    }

}