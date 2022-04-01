// ReSharper disable ClassNeverInstantiated.Global

using System.Diagnostics.CodeAnalysis;

namespace Nuages.Identity.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class ConfigOptions
{
    public string StackName { get; set; } = string.Empty;

    public string DomainName { get; set; } = string.Empty;

    public string CertificateArn { get; set; } = string.Empty;
    
    public DbProxy DatabaseDbProxy { get; set; } = new();
    
    public string? VpcId { get; set; }
    public string? SecurityGroupId { get; set; }
}

[ExcludeFromCodeCoverage]
public class DbProxy
{
    public string? Arn { get; set; }
    public string? Name { get; set; }
    public string? Endpoint { get; set; }
    public  string? UserName { get; set; }
}