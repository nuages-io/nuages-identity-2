namespace Nuages.Identity.Services.Email.Sender;

public interface IMessageService
{
    void SendEmailUsingTemplate(
        string to,
        string template,
        IDictionary<string, string>? fields = null,
        string? language = null);

    void SendSms(string to, string text);
}