using Nuages.Identity.Services.Email.Sender;
// ReSharper disable UnusedVariable

namespace Nuages.Identity.UI.Tests;

public class DummyEmailMessageSender : IEmailMessageSender
{
   
    public Task<string> SendEmailUsingTemplateAsync(string from, string to, string template, string? language,
        IDictionary<string, string>? fields = null)
    {
        return Task.FromResult("");
    }
}