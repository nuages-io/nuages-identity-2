using System.Text.Json;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Nuages.Identity.Services.Email.Sender;

// ReSharper disable InconsistentNaming

namespace Nuages.Identity.UI.AWS;

// ReSharper disable once InconsistentNaming
public class AWSSender : MessageSender
{
    private readonly IAmazonSimpleEmailServiceV2 _emailServiceV2;
    private readonly IAmazonSimpleNotificationService _notificationService;

    public AWSSender(IAmazonSimpleEmailServiceV2 emailServiceV2, IAmazonSimpleNotificationService notificationService )
    {
        _emailServiceV2 = emailServiceV2;
        _notificationService = notificationService;
    }
    
    public override async Task<string> SendEmailUsingTemplateAsync(string from, string to, string template, string? language,
        IDictionary<string, string> fields)
    {
        var data = JsonSerializer.Serialize(fields);
        
        var request = new SendEmailRequest
        {
            ConfigurationSetName = null,
            Content = new EmailContent
            {
                Template = new Template
                {
                    TemplateName =  $"{template}_{language}",
                    TemplateData = data
                }
            },
            Destination = new Destination
            {
                ToAddresses = new List<string> { to }
            },
            FromEmailAddress = from
        };

        try
        {
            var res = await _emailServiceV2.SendEmailAsync(request);

            return res.MessageId;
        }
        catch (Amazon.SimpleEmailV2.Model.NotFoundException e)
        {
            Console.WriteLine(e.Message);
            
            return await base.SendEmailUsingTemplateAsync(from, to, template, language, fields);
        }
      
    }

    public override async Task<string> SendSmsAsync(string to, string text)
    {
        var response = await _notificationService.PublishAsync(new PublishRequest
        {
            Message = text,
            PhoneNumber = to
        });

        return response.MessageId;
    }
}

public static class AWSSenderExtension
{
    public static void AddAWSSender(this IServiceCollection services, string templateFileName, bool initializeTemplate = false)
    {
        services.AddAWSService<IAmazonSimpleEmailServiceV2>();
        services.AddAWSService<IAmazonSimpleNotificationService>();
        
        services.AddScoped<IEmailMessageSender, AWSSender>();
        services.AddScoped<ISmsMessageSender, AWSSender>();

        if (initializeTemplate)
        {
            services.AddHostedService(serviceProvider =>
                new SesTemplateInitializer(serviceProvider.GetRequiredService<IAmazonSimpleEmailServiceV2>(), templateFileName)
            );
        }
        
    }
}