using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.Route53;
using CfnDomainName = Amazon.CDK.AWS.Apigatewayv2.CfnDomainName;
using CfnDomainNameProps = Amazon.CDK.AWS.Apigatewayv2.CfnDomainNameProps;

// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Nuages.Identity.CDK;

public partial class IdentityCdkStack
{
    private void CreateUILambda()
    {
        var role = CreateUILambdaRole();

        if (string.IsNullOrEmpty(AssetUi)) throw new Exception("AssetUi must be assigned");

        // ReSharper disable once UnusedVariable
        var func = new Function(this, "WebUI", new FunctionProps
        {
            FunctionName = MakeId("WebUI"),
            Code = Code.FromAsset(AssetUi),
            Handler = "Nuages.Identity.UI",
            Runtime = new Runtime("dotnet6"),
            Role = role,
            Timeout = Duration.Seconds(30),
            Environment = new Dictionary<string, string>
            {
                { "Nuages__Identity__StackName", StackName }
            },
            Tracing = Tracing.ACTIVE,
            MemorySize = 2048,
            AllowPublicSubnet = true,
            Vpc = CurrentVpc,
            SecurityGroups = VpcApiSecurityGroups
        });

        func.AddEventSource(new ApiEventSource("ANY", "/{proxy+}"));

        func.AddEventSource(new ApiEventSource("ANY", "/"));

        var webApi = (RestApi)Node.Children.Single(c =>
            c.GetType() == typeof(RestApi) && ((RestApi)c).RestApiName.Contains("WebUI"));

        if (!string.IsNullOrEmpty(DomainName))
        {
            var apiGatewayDomainName = new CfnDomainName(this, "NuagesUIDomainName", new CfnDomainNameProps
            {
                DomainName = DomainName,
                DomainNameConfigurations = new[]
                {
                    new CfnDomainName.DomainNameConfigurationProperty
                    {
                        EndpointType = "REGIONAL",
                        CertificateArn = CertificateArn
                    }
                }
            });

            var hostedZone = HostedZone.FromLookup(this, "LookupUI", new HostedZoneProviderProps
            {
                DomainName = GetBaseDomain(DomainName)
            });

            // ReSharper disable once UnusedVariable
            var recordSet = new CfnRecordSet(this, "Route53RecordSetGroupUI", new CfnRecordSetProps
            {
                AliasTarget = new CfnRecordSet.AliasTargetProperty
                {
                    DnsName = apiGatewayDomainName.AttrRegionalDomainName,
                    HostedZoneId = apiGatewayDomainName.AttrRegionalHostedZoneId
                },
                HostedZoneId = hostedZone.HostedZoneId,
                Name = DomainName,
                Type = "A"
            });

            // ReSharper disable once UnusedVariable
            var apiMapping = new CfnApiMapping(this, MakeId("NuagesIdentityUIMapping"), new CfnApiMappingProps
            {
                DomainName = apiGatewayDomainName.DomainName,
                ApiId = webApi.RestApiId,
                Stage = webApi.DeploymentStage.StageName
            });

            // ReSharper disable once UnusedVariable
            var output = new CfnOutput(this, "NuagesIdentityUI", new CfnOutputProps
            {
                Value = $"https://{apiGatewayDomainName.DomainName}",
                Description = "Custom Url for the Web UI"
            });
        }
        else
        {
            throw new Exception("DomainName must be provided");
        }
    }

    private Role CreateUILambdaRole()
    {
        var role = new Role(this, "RoleWebUI", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });

        role.AddManagedPolicy(CreateLambdaExecutionRolePolicy("UI"));
        role.AddManagedPolicy(CreateLambdaFullAccessRolePolicy("UI"));
        role.AddManagedPolicy(CreateSystemsManagerParametersRolePolicy("UI"));
        role.AddManagedPolicy(CreateS3RolePolicy("UI"));
        role.AddManagedPolicy(CreateSESRolePolicy("UI"));
        role.AddManagedPolicy(CreateSnsRolePolicy("UI"));
        role.AddManagedPolicy(CreateXrayRolePolicy("UI"));
        role.AddManagedPolicy(CreateSecretsManagerPolicy("UI"));
        
        return role;
    }

}