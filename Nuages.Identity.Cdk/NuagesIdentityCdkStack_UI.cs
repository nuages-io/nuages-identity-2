using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Route53;
using CfnDomainName = Amazon.CDK.AWS.Apigatewayv2.CfnDomainName;
using CfnDomainNameProps = Amazon.CDK.AWS.Apigatewayv2.CfnDomainNameProps;

// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Nuages.Identity.CDK;

public partial class NuagesIdentityCdkStack
{
    private void CreateWebUi()
    {
        var role = CreateWebUiRole();

        if (string.IsNullOrEmpty(AssetUi)) throw new Exception("AssetUi must be assigned");

        // ReSharper disable once UnusedVariable
        var func = new Function(this, "WebUI", new FunctionProps
        {
            FunctionName = MakeId("WebUI"),
            Code = Code.FromAsset(AssetUi),
            Handler = "Nuages.Identity.UI::Nuages.Identity.UI.LambdaEntryPoint::FunctionHandlerAsync",
            Runtime = new Runtime("dotnet6"),
            Role = role,
            Timeout = Duration.Seconds(30),
            Environment = new Dictionary<string, string>
            {
                { "Nuages__Identity__StackName", StackName }
            },
            Tracing = Tracing.ACTIVE,
            MemorySize = 2048
        });

        func.AddEventSource(new ApiEventSource("ANY", "/{proxy+}"));

        func.AddEventSource(new ApiEventSource("ANY", "/"));


        var webApi = (RestApi)Node.Children.Single(c =>
            c.GetType() == typeof(RestApi) && ((RestApi)c).RestApiName.Contains("WebUI"));

        // var apiDomain = $"{webApi.RestApiId}.execute-api.{Aws.REGION}.amazonaws.com";
        // var apiCheckPath = $"{webApi.DeploymentStage.StageName}/health";
        //
        //  var hc = new CfnHealthCheck(this, MakeId("HealthCheckUI"), new CfnHealthCheckProps
        //  {
        //      HealthCheckConfig = new CfnHealthCheck.HealthCheckConfigProperty
        //      {
        //          EnableSni = true,
        //          FailureThreshold = 3,
        //          FullyQualifiedDomainName = apiDomain,
        //          Port = 443,
        //          RequestInterval = 30,
        //          ResourcePath = apiCheckPath,
        //          Type = "HTTPS"
        //      },
        //      //©©HealthCheckTags = null
        //  });

        var domainName = (string)Node.TryGetContext("DomainName");

        if (!string.IsNullOrEmpty(domainName))
        {
            var certficateArn = (string)Node.TryGetContext("CertificateArn");

            var apiGatewayDomainName = new CfnDomainName(this, "NuagesUIDomainName", new CfnDomainNameProps
            {
                DomainName = domainName,
                DomainNameConfigurations = new[]
                {
                    new CfnDomainName.DomainNameConfigurationProperty
                    {
                        EndpointType = "REGIONAL",
                        CertificateArn = certficateArn
                    }
                }
            });

            var hostedZone = HostedZone.FromLookup(this, "LookupUI", new HostedZoneProviderProps
            {
                DomainName = GetBaseDomain(domainName)
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
                Name = domainName,
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

    private Role CreateWebUiRole()
    {
        var role = new Role(this, "RoleWebUI", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });

        role.AddManagedPolicy(CreateLambdaBasicExecutionRolePolicy("UI"));
        role.AddManagedPolicy(CreateLambdaFullAccessRolePolicy("UI"));
        role.AddManagedPolicy(CreateSystemsManagerParametersRolePolicy("UI"));
        role.AddManagedPolicy(CreateS3RolePolicy("UI"));
        role.AddManagedPolicy(CreateSESRolePolicy("UI"));
        role.AddManagedPolicy(CreateSnsRolePolicy("UI"));
        role.AddManagedPolicy(CreateXrayRolePolicy("UI"));
        return role;
    }
}