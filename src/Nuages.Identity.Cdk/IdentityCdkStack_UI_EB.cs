using Amazon.CDK.AWS.ElasticBeanstalk;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3.Assets;

namespace Nuages.Identity.CDK;

public partial class IdentityCdkStack
{
    private void CreateEB()
    {
        var zip = new Asset(this, MakeId("Zip"), new AssetProps
        {
            Path = AssetUi + ".zip"
        });
        
        var application = new CfnApplication(this, MakeId("NuagesIDentityEB"), new CfnApplicationProps
        {
            ApplicationName = StackName,
            Description = null,
            ResourceLifecycleConfig = null
        });

        var version = new CfnApplicationVersion(this, MakeId("NuagesIdentityEBVersion"), new CfnApplicationVersionProps
        {
            ApplicationName = StackName,
            SourceBundle = new CfnApplicationVersion.SourceBundleProperty
            {
                S3Bucket = zip.S3BucketName,
                S3Key = zip.S3ObjectKey
            },
            Description = null
        });
        
        version.AddDependsOn(application);
        
        var myRole = new Role(this, MakeId("-aws-elasticbeanstalk-ec2-role"),new RoleProps() {
            AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
        });

        var managedPolicy = ManagedPolicy.FromAwsManagedPolicyName("AWSElasticBeanstalkWebTier");
        myRole.AddManagedPolicy(managedPolicy);

        var myProfileName = $"{StackName}-InstanceProfile";

        var instanceProfile = new CfnInstanceProfile(this, myProfileName, new CfnInstanceProfileProps {
            InstanceProfileName =  myProfileName,
            Roles = new  []
            {
                myRole.RoleName
            }
        });

        var optionSettingProperties = new CfnEnvironment.OptionSettingProperty []
        {
           new()
           {
           Namespace = "aws:autoscaling:launchconfiguration",
           OptionName = "IamInstanceProfile",
           Value = myProfileName,
           },
           new()
           {
               Namespace = "aws:autoscaling:asg",
               OptionName = "MinSize",
               Value = "1",
           },
           new()
           {
               Namespace = "aws:autoscaling:asg",
               OptionName= "MaxSize",
               Value = "1",
           },
           new()
           {
               Namespace = "aws:ec2:instances",
               OptionName =  "InstanceTypes",
               Value = "t2.micro",
           }
        };

        var elbEnv = new CfnEnvironment(this, MakeId("Environment"), new CfnEnvironmentProps {
            EnvironmentName =  $"{StackName}-Env",
            ApplicationName =  StackName,
            SolutionStackName = "64bit Amazon Linux 2 v2.3.0 running .NET Core",
            OptionSettings = optionSettingProperties,
            VersionLabel = version.Ref
        });
    }
}