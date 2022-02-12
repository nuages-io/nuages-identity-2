using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Constructs;
using Nuages.Identity.CDK;

namespace Nuages.Identity.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class MyNuagesSenderCdkStack : NuagesIdentityCdkStack
{
    public MyNuagesSenderCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        AssetUi = "./Nuages.Identity.UI/bin/Release/net6.0/linux-x64/publish";
        AssetApi = "./Nuages.Identity.API/bin/Release/net6.0/linux-x64/publish";
    }
}