using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Localization.CultureProvider;

namespace Nuages.Identity.Services.Email.Sender;

public class MessageService : IMessageService
{
    private readonly NuagesIdentityOptions _options;
    private readonly IServiceProvider _provider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly MessageServiceOptions _serviceOptions;

    public MessageService(IOptions<NuagesIdentityOptions> options, IServiceProvider provider,
        IServiceScopeFactory serviceScopeFactory, IOptions<MessageServiceOptions> senderOptions)
    {
        _provider = provider;
        _serviceScopeFactory = serviceScopeFactory;
        _serviceOptions = senderOptions.Value;
        _options = options.Value;
    }

    public void SendEmailUsingTemplate(string to, string template, IDictionary<string, string>? fields = null,
        string? language = null)
    {
        if (to.ToLower().Contains("@example.com"))
            return;
        
        fields ??= new Dictionary<string, string>();

        if (!fields.ContainsKey("AppName")) fields["AppName"] = _options.Name;

        if (string.IsNullOrEmpty(language)) language = GetLanguage();

        Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var messageSender = scope.ServiceProvider.GetRequiredService<IEmailMessageSender>();
            await messageSender.SendEmailUsingTemplateAsync(_serviceOptions.SendFromEmail, to, template, language, fields);
        });
    }

    public void SendSms(string to, string text)
    {
        Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var messageSender = scope.ServiceProvider.GetRequiredService<ISmsMessageSender>();
            await messageSender.SendSmsAsync(to, text);
        });
    }

    private string GetLanguage()
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

        if (string.IsNullOrEmpty(culture))
            culture = _serviceOptions.DefaultCulture;
        
        return culture!.Split("-").First();
    }
}