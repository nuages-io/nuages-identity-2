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
    private void CreateWebApi()
    {
        var role = CreateWebApiRole();

        if (string.IsNullOrEmpty(AssetApi)) throw new Exception("AssetApi must be assigned");

        // ReSharper disable once UnusedVariable
        var func = new Function(this, "WebAPI", new FunctionProps
        {
            FunctionName = MakeId("WebAPI"),
            Code = Code.FromAsset(AssetApi),
            Handler = "Nuages.Identity.API::Nuages.Identity.API.LambdaEntryPoint::FunctionHandlerAsync",
            Runtime = Runtime.DOTNET_CORE_3_1,
            Role = role,
            Timeout = Duration.Seconds(30),
            Environment = new Dictionary<string, string>
            {
                { "Nuages__Identity__StackName", StackName }
            },
            Tracing = Tracing.ACTIVE
        });

        func.AddEventSource(new ApiEventSource("ANY", "/{proxy+}"));
        func.AddEventSource(new ApiEventSource("ANY", "/"));

        var webApi = (RestApi)Node.Children.Single(c =>
            c.GetType() == typeof(RestApi) && ((RestApi)c).RestApiName.Contains("WebAPI"));

        // var apiDomain = $"{webApi.RestApiId}.execute-api.{Aws.REGION}.amazonaws.com";
        // var apiCheckPath = $"{webApi.DeploymentStage.StageName}/health";
        //
        //  var hc = new CfnHealthCheck(this, MakeId("HealthCheckAPI"), new CfnHealthCheckProps
        //  {
        //      HealthCheckConfig = new CfnHealthCheck.HealthCheckConfigProperty
        //      {
        //          EnableSni = true,
        //          FailureThreshold = 3,
        //          FullyQualifiedDomainName = apiDomain,
        //          Port = 443,
        //          RequestInterval = 30,
        //          ResourcePath = apiCheckPath,
        //          Type = "HTTPS",
        //      },
        //      //©©HealthCheckTags = null
        //  });

        var domainName = (string)Node.TryGetContext("DomainNameApi");

        if (!string.IsNullOrEmpty(domainName))
        {
            var certficateArn = (string)Node.TryGetContext("CertificateArn");

            var apiGatewayDomainName = new CfnDomainName(this, "NuagesApiDomainName", new CfnDomainNameProps
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

            var hostedZone = HostedZone.FromLookup(this, "LookupApi", new HostedZoneProviderProps
            {
                DomainName = GetBaseDomain(domainName)
            });

            // ReSharper disable once UnusedVariable
            var recordSet = new CfnRecordSet(this, "Route53RecordSetGroupApi", new CfnRecordSetProps
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
            var apiMapping = new CfnApiMapping(this, MakeId("NuagesIdentityApiMapping"), new CfnApiMappingProps
            {
                DomainName = apiGatewayDomainName.DomainName,
                ApiId = webApi.RestApiId,
                Stage = webApi.DeploymentStage.StageName
            });

            // ReSharper disable once UnusedVariable
            var output = new CfnOutput(this, "NuagesIdentityApi", new CfnOutputProps
            {
                Value = $"https://{apiGatewayDomainName.DomainName}",
                Description = "Custom Url for the Web API"
            });
        }
        else
        {
            throw new Exception("DomainName must be provided");
        }
    }

    private Role CreateWebApiRole()
    {
        var role = new Role(this, "RoleWebApi", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });

        role.AddManagedPolicy(CreateLambdaBasicExecutionRolePolicy("API"));
        role.AddManagedPolicy(CreateLambdaFullAccessRolePolicy("API"));
        role.AddManagedPolicy(CreateSystemsManagerParametersRolePolicy("API"));
        role.AddManagedPolicy(CreateS3RolePolicy("API"));
        role.AddManagedPolicy(CreateSESRolePolicy("API"));
        role.AddManagedPolicy(CreateSnsRolePolicy("API"));
        role.AddManagedPolicy(CreateXrayRolePolicy("API"));

        return role;
    }
}