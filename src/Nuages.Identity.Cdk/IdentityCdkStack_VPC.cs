using Amazon.CDK.AWS.EC2;

namespace Nuages.Identity.CDK;

public partial class IdentityCdkStack
{
    public string? VpcId { get; set; }
    public string? SecurityGroupId { get; set; }
    
    private IVpc? _vpc;

    private ISecurityGroup? _securityGroup;
    private ISecurityGroup? _vpcSecurityGroup;
    
    private IVpc? CurrentVpc
    {
        get
        {
            if (!string.IsNullOrEmpty(VpcId) && _vpc == null)
            {
                Console.WriteLine("Vpc.FromLookup");
                _vpc = Vpc.FromLookup(this, "Vpc", new VpcLookupOptions
                {
                    VpcId = VpcId
                });
            }

            return _vpc;
        }
    }

    private ISecurityGroup? SecurityGroup
    {
        get
        {
            if (_securityGroup == null && !string.IsNullOrEmpty(SecurityGroupId))
                _securityGroup =
                    Amazon.CDK.AWS.EC2.SecurityGroup.FromLookupById(this, "WebApiSGDefault", SecurityGroupId!);

            return _securityGroup;
        }
    }

    private ISecurityGroup[] SecurityGroups
    {
        get
        {
            if (_vpcSecurityGroup == null && !string.IsNullOrEmpty(VpcId))
            {
                _vpcSecurityGroup ??= CreateVpcSecurityGroup();
            }

            var list = new List<ISecurityGroup>();

            if (_vpcSecurityGroup != null)
                list.Add(_vpcSecurityGroup);

            if (SecurityGroup != null)
                list.Add(SecurityGroup);

            return list.ToArray();
        }
    }

    protected virtual SecurityGroup CreateVpcSecurityGroup()
    {
        return new SecurityGroup(this, MakeId("WSSSecurityGroup"), new SecurityGroupProps
        {
            Vpc = CurrentVpc!,
            AllowAllOutbound = true,
            Description = "PubSub WSS Security Group"
        });
    }

    private ISecurityGroup? _vpcApiSecurityGroup;

    private ISecurityGroup[] VpcApiSecurityGroups
    {
        get
        {
            if (_vpcApiSecurityGroup == null && !string.IsNullOrEmpty(VpcId))
            {
                _vpcApiSecurityGroup ??= CreateVpcApiSecurityGroup();
            }

            var list = new List<ISecurityGroup>();

            if (_vpcApiSecurityGroup != null)
                list.Add(_vpcApiSecurityGroup);

            if (SecurityGroup != null)
                list.Add(SecurityGroup);

            return list.ToArray();
        }
    }

    protected virtual SecurityGroup CreateVpcApiSecurityGroup()
    {
        Console.WriteLine($"CreateVpcApiSecurityGroup");

        return new SecurityGroup(this, MakeId("ApiSecurityGroup"), new SecurityGroupProps
        {
            Vpc = CurrentVpc!,
            AllowAllOutbound = true,
            Description = "PubSub API Security Group"
        });
    }
}