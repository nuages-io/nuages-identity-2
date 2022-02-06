

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Cdk.Deploy;


public class ConfigOptions
{
    public string StackName { get; set; } = string.Empty;

    public string DomainName { get; set; } = string.Empty;
    public string DomainNameApi { get; set; } = string.Empty;
    
    public string CertificateArn { get; set; } = string.Empty;
}