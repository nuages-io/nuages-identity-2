// ReSharper disable ClassNeverInstantiated.Global

using System.Diagnostics.CodeAnalysis;

namespace Nuages.Identity.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class ConfigOptions
{
    public string StackName { get; set; } = string.Empty;

    public string DomainName { get; set; } = string.Empty;

    public string CertificateArn { get; set; } = string.Empty;
}
