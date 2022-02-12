using Nuages.Sender.API.Sdk;

namespace Nuages.Identity.UI.Tests;

public class DummyMessageSender : IMessageSender
{
    public Task<string> SendEmailUsingTemplateAsync(string to, string template,
        IDictionary<string, string>? fields = null, string? language = null,
        string? from = null)
    {
        return Task.FromResult("");
    }

    public Task SendSmsAsync(string to, string text)
    {
        return Task.CompletedTask;
    }
}