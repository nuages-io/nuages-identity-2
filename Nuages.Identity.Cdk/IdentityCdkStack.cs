using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SES;
using Constructs;
using HtmlAgilityPack;
// ReSharper disable ObjectCreationAsStatement

namespace Nuages.Identity.CDK;

[ExcludeFromCodeCoverage]
public partial class IdentityCdkStack : Stack
{
    // ReSharper disable once MemberCanBeProtected.Global
    public IdentityCdkStack(Construct scope, string id, IStackProps props) : base(scope, id, props)
    {
    }

    protected string AssetUi { get; set; } = "";
    protected string AssetApi { get; set; } = "";

    public string? TemplateFileName { get; set; }
    
    protected void CreateTemplate()
    {
        CreateWebUi();

        CreateEmailTemplates();
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

    protected virtual void CreateEmailTemplates()
    {
        if (!string.IsNullOrEmpty(TemplateFileName))
        {
            var json = File.ReadAllText(TemplateFileName);
            var data = JsonSerializer.Deserialize<List<EmailTemplate>>(json);

            if (data != null)
            {
                foreach (var t in data.OrderBy(t => t.Key))
                {
                    foreach (var d in t.Data)
                    {
                        var key = t.Key + "_" + d.Language;
                
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(d.EmailHtml);
                
                        new CfnTemplate(this, MakeId(key), new CfnTemplateProps
                        {
                            Template = new CfnTemplate.TemplateProperty
                            {
                                TemplateName = key,
                                //HtmlPart =  WebUtility.HtmlEncode(d.EmailHtml),
                                SubjectPart = d.EmailSubject,
                                TextPart = htmlDoc.DocumentNode.InnerText
                            }
                        });
                    }
                }
            }
        }
        
    }
    
    private string MakeId(string id)
    {
        return $"{StackName}-{id}";
    }
}