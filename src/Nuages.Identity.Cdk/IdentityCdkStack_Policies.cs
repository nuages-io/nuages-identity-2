using Amazon.CDK.AWS.IAM;

namespace Nuages.Identity.CDK;

public partial class IdentityCdkStack
{
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
                        Actions = new[] { "ssm:GetParametersByPath", "appconfig:GetConfiguration" },
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
                        Actions = new[] { "secretsmanager:GetSecretValue" },
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
    
    
    private ManagedPolicy CreateLambdaExecutionRolePolicy(string suffix)
    {
        var permissions = new List<string>
        {
            "logs:CreateLogGroup",
            "logs:CreateLogStream",
            "logs:PutLogEvents"
        };

        if (!string.IsNullOrEmpty(VpcId))
        {
            permissions.AddRange(new List<string>
            {
                "ec2:DescribeNetworkInterfaces",
                "ec2:CreateNetworkInterface",
                "ec2:DeleteNetworkInterface",
                "ec2:DescribeInstances",
                "ec2:AttachNetworkInterface"
            });
        }
        
        return new ManagedPolicy(this, MakeId("LambdaBasicExecutionRole" + suffix), new ManagedPolicyProps
        {
        
            Document = new PolicyDocument(new PolicyDocumentProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = permissions.ToArray(),
                        Resources = new[] { "*" }
                    })
                }
            })
        });
    }

}