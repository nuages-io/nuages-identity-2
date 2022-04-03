using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.RDS;
using Constructs;
// ReSharper disable VirtualMemberNeverOverridden.Global

// ReSharper disable ObjectCreationAsStatement

namespace Nuages.Identity.CDK;

[ExcludeFromCodeCoverage]
public partial class IdentityCdkStack : Stack
{
    // ReSharper disable once MemberCanBeProtected.Global
    public IdentityCdkStack(Construct scope, string id, IStackProps props) : base(scope, id, props)
    {
    }

    protected string AssetUi { get; set; } = "";
    
    public string? CertificateArn { get; set; }
    public string? DomainName { get; set; }
   
    protected void CreateTemplate()
    {
        CreateUILambda();
        //CreateEB();
    }
    
    private static string GetBaseDomain(string domainName)
    {
        var tokens = domainName.Split('.');

        if (tokens.Length != 3)
            return domainName;

        var tok = new List<string>(tokens);
        var remove = tokens.Length - 2;
        tok.RemoveRange(0, remove);

        return tok[0] + "." + tok[1];
    }

    private string MakeId(string id)
    {
        return $"{StackName}-{id}";
    }
}