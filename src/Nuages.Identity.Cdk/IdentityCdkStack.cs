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

    public string? CertificateArn { get; set; }

    public string? DomainName { get; set; }

    public string? VpcId { get; set; }
    public string? SecurityGroupId { get; set; }
    
    protected string AssetUi { get; set; } = "";
    
    protected IVpc? _vpc;

    private ISecurityGroup? _securityGroup;
    private ISecurityGroup? _vpcSecurityGroup;
    
    protected void CreateTemplate()
    {
        CreateWebUi();
    }
    
    private ManagedPolicy CreateLambdaBasicExecutionRolePolicy(string suffix)
    {
        return new ManagedPolicy(this, MakeId("LambdaBasicExecutionRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[]
                        {
                            "logs:CreateLogGroup",
                            "logs:CreateLogStream",
                            "logs:PutLogEvents"
                        },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
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

    // ReSharper disable once InconsistentNaming
    private ManagedPolicy CreateSESRolePolicy(string suffix)
    {
        return new ManagedPolicy(this, MakeId("SESRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "ses:*" },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }

    private ManagedPolicy CreateSnsRolePolicy(string suffix)
    {
        return new ManagedPolicy(this, MakeId("SNSRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "sns:*" },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }

    private ManagedPolicy CreateSystemsManagerParametersRolePolicy(string suffix)
    {
        return new ManagedPolicy(this, MakeId("SystemsManagerParametersRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "ssm:GetParametersByPath", "appconfig:GetConfiguration"  },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }
    
    private ManagedPolicy CreateS3RolePolicy(string suffix)
    {
        return new ManagedPolicy(this, MakeId("S3Role" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[]
                        {
                            "s3:*",
                            "s3-object-lambda:*"
                        },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }

    protected virtual ManagedPolicy CreateSecretsManagerPolicy(string suffix = "")
    {
        return new ManagedPolicy(this, MakeId("SecretsManagerRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] {  "secretsmanager:GetSecretValue" },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }
    
    private ManagedPolicy CreateXrayRolePolicy(string suffix)
    {
        return new ManagedPolicy(this, MakeId("Xray" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "xray:*" },
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }

    private IManagedPolicy CreateLambdaFullAccessRolePolicy(string suffix)
    {
        return new ManagedPolicy(this, MakeId("LambdaFullAccessRole" + suffix), new ManagedPolicyProps
        {
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[]
                        {
                            "cloudformation:DescribeStacks",
                            "cloudformation:ListStackResources",
                            "cloudwatch:ListMetrics",
                            "cloudwatch:GetMetricData",
                            "ec2:DescribeSecurityGroups",
                            "ec2:DescribeSubnets",
                            "ec2:DescribeVpcs",
                            "kms:ListAliases",
                            "iam:GetPolicy",
                            "iam:GetPolicyVersion",
                            "iam:GetRole",
                            "iam:GetRolePolicy",
                            "iam:ListAttachedRolePolicies",
                            "iam:ListRolePolicies",
                            "iam:ListRoles",
                            "lambda:*",
                            "logs:DescribeLogGroups",
                            "states:DescribeStateMachine",
                            "states:ListStateMachines",
                            "tag:GetResources",
                            "xray:GetTraceSummaries",
                            "xray:BatchGetTraces"
                        },
                        Resources = new[] { "*" }
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "iam:PassRole" },
                        Resources = new[] { "*" },
                        Conditions = new Dictionary<string, object>
                        {
                            {
                                "StringEquals", new Dictionary<string, string>
                                {
                                    { "iam:PassedToService", "lambda.amazonaws.com" }
                                }
                            }
                        }
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[]
                        {
                            "logs:DescribeLogStreams",
                            "logs:GetLogEvents",
                            "logs:FilterLogEvents"
                        },
                        Resources = new[] { "arn:aws:logs:*:*:log-group:/aws/lambda/*" }
                    })
                }
            })
        });
    }
    
    private string MakeId(string id)
    {
        return $"{StackName}-{id}";
    }
    
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
                _securityGroup = Amazon.CDK.AWS.EC2.SecurityGroup.FromLookupById(this, "WebApiSGDefault", SecurityGroupId!);

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