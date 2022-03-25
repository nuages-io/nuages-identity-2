

using System.Text.Json;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using HtmlAgilityPack;

namespace Nuages.Identity.UI.AWS;

public class SesTemplateInitializer : BackgroundService
{
    private readonly IAmazonSimpleEmailServiceV2 _simpleEmailServiceV2;

    public SesTemplateInitializer(IAmazonSimpleEmailServiceV2 simpleEmailServiceV2)
    {
        _simpleEmailServiceV2 = simpleEmailServiceV2;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var json = await File.ReadAllTextAsync("templates.json", stoppingToken);
        var data = JsonSerializer.Deserialize<List<EmailTemplate>>(json);

        if (data != null)
        {
            foreach (var t in data)
            {
                foreach (var d in t.Data)
                {
                    var key = t.Key + "_" + d.Language;
                
                    var request = new GetEmailTemplateRequest
                    {
                        TemplateName = key
                    };

                    try
                    {
                        await _simpleEmailServiceV2.GetEmailTemplateAsync(request, stoppingToken);
                    }
                    catch (NotFoundException e)
                    {
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(d.EmailHtml);
                    
                        await _simpleEmailServiceV2.CreateEmailTemplateAsync(new CreateEmailTemplateRequest
                        {
                            TemplateName = key,
                            TemplateContent = new EmailTemplateContent
                            {
                                Html = d.EmailHtml,
                                Subject = d.EmailSubject,
                                Text = htmlDoc.DocumentNode.InnerHtml
                            }
                        }, stoppingToken);
                    }
                }
            }
        }
        
    }
}