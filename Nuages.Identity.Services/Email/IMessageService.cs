using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Nuages.Sender.API.Sdk;

namespace Nuages.Identity.Services.Email;

public class MessageService : IMessageService
{
    private readonly IMessageSender _messageSender;
    private readonly IRequestCultureProvider _cultureProvider;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly NuagesIdentityOptions _options;

    public MessageService(IMessageSender messageSender, IOptions<NuagesIdentityOptions> options, IRequestCultureProvider cultureProvider, IHttpContextAccessor contextAccessor)
    {
        _messageSender = messageSender;
        _cultureProvider = cultureProvider;
        _contextAccessor = contextAccessor;
        _options = options.Value;
    }

    public async Task<string> SendEmailUsingTemplateAsync(string to, string template, IDictionary<string, string>? fields = null, string? language = null)
    {
        fields ??= new Dictionary<string, string>();

        if (!fields.ContainsKey("AppName"))
        {
            fields["AppName"] = _options.Name;
        }

        if (string.IsNullOrEmpty(language))
        {
            language = ((await _cultureProvider.DetermineProviderCultureResult(_contextAccessor.HttpContext!))!).UICultures.First().Value;
        }
        
        return await _messageSender.SendEmailUsingTemplateAsync(to, template, fields, language);
    }

    public async Task SendSmsAsync(string to, string text)
    {
        await _messageSender.SendSmsAsync(to, text);
    }
}

public interface IMessageService
{
    Task<string> SendEmailUsingTemplateAsync(
        string to,
        string template,
        IDictionary<string, string>? fields = null,
        string? language = null);
    
    Task SendSmsAsync(string to, string text);
}