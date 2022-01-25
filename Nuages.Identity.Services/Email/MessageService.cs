using Microsoft.Extensions.Options;
using Nuages.Localization.CultureProvider;
using Nuages.Sender.API.Sdk;

namespace Nuages.Identity.Services.Email;

public class MessageService : IMessageService
{
    private readonly IServiceProvider _provider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly NuagesIdentityOptions _options;

    public MessageService(IOptions<NuagesIdentityOptions> options, IServiceProvider provider, IServiceScopeFactory serviceScopeFactory)
    {
        _provider = provider;
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
    }

    public void SendEmailUsingTemplate(string to, string template, IDictionary<string, string>? fields = null, string? language = null)
    {
        fields ??= new Dictionary<string, string>();

        if (!fields.ContainsKey("AppName"))
        {
            fields["AppName"] = _options.Name;
        }

        if (string.IsNullOrEmpty(language))
        {
            language = GetLanguage();
        }

        Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var messageSender = scope.ServiceProvider.GetRequiredService<IMessageSender>();
            await messageSender.SendEmailUsingTemplateAsync(to, template, fields, language);
        });
    }

    private string? GetLanguage()
    {
        var cultureProviders = _provider.GetServices<ICultureProvider>();

        string? culture = null;
                
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var provider in cultureProviders.Reverse())
        {
            culture = provider.GetCulture();
            if (!string.IsNullOrEmpty(culture))
                break;
        }

        return culture;
    }

    public void SendSms(string to, string text)
    {
        
#pragma warning disable CS4014
        Task.Run(async () =>
#pragma warning restore CS4014
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var messageSender = scope.ServiceProvider.GetRequiredService<IMessageSender>();
            await messageSender.SendSmsAsync(to, text);
        });
        
    }
}

public interface IMessageService
{
    void SendEmailUsingTemplate(
        string to,
        string template,
        IDictionary<string, string>? fields = null,
        string? language = null);
    
    void SendSms(string to, string text);
}