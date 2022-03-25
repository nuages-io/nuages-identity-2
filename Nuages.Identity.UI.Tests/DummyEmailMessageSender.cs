using Nuages.Identity.Services.Email.Sender;

namespace Nuages.Identity.UI.Tests;

public class DummyEmailMessageSender : IEmailMessageSender
{
    public Task SendSmsAsync(string to, string text)
    {
        return Task.CompletedTask;
    }

    public Task<string> SendEmailUsingTemplateAsync(string from, string to, string template, string? language,
        IDictionary<string, string>? fields = null)
    {
        return Task.FromResult("");
    }
}