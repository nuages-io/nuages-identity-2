using System.Diagnostics.CodeAnalysis;
using Amazon.SimpleEmailV2;
using Amazon.SimpleNotificationService;
using Nuages.Identity.Services.Email.Sender;
// ReSharper disable InconsistentNaming

namespace Nuages.Identity.AWS.Sender;

public static class AWSSenderExtension
{
    public static void AddAWSSender(this IServiceCollection services, string templateFileName, bool initializeTemplate = false)
    {
        services.AddAWSService<IAmazonSimpleEmailServiceV2>();
        services.AddAWSService<IAmazonSimpleNotificationService>();
        
        services.AddScoped<IEmailMessageSender, AWSSender>();
        services.AddScoped<ISmsMessageSender, AWSSender>();

        InitializeTemplate(services, templateFileName, initializeTemplate);
        
    }

    [ExcludeFromCodeCoverage]
    private static void InitializeTemplate(IServiceCollection services, string templateFileName, bool initializeTemplate)
    {
        if (initializeTemplate)
        {
            services.AddHostedService(serviceProvider =>
                new SesTemplateInitializer(serviceProvider.GetRequiredService<IAmazonSimpleEmailServiceV2>(),
                    templateFileName)
            );
        }
    }
}