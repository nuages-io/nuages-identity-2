using System.Diagnostics.CodeAnalysis;
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
        AssetUi = "./Nuages.Identity.UI/bin/Release/net6.0/linux-x64/publish";
        AssetApi = "./Nuages.Identity.API/bin/Release/net6.0/linux-x64/publish";
    }
    
    public static void CreateStack(Construct scope, IConfiguration configuration)
    {
        var options = configuration.Get<ConfigOptions>();
        
        var stack = new IdentityStack(scope, "Stack", new StackProps
        {
            StackName = options.StackName,
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        });

        stack.Node.SetContext("DomainName", options.DomainName);
        stack.Node.SetContext("DomainNameApi", options.DomainNameApi);
        stack.Node.SetContext("CertificateArn", options.CertificateArn);

        stack.CreateTemplate();
    }
}