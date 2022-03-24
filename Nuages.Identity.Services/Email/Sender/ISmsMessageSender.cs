namespace Nuages.Identity.Services.Email.Sender;

public interface ISmsMessageSender
{
    Task<string> SendSmsAsync(string to, string text);
}