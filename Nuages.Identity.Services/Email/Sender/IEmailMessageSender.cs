namespace Nuages.Identity.Services.Email.Sender;

public interface IEmailMessageSender
{
    Task<string> SendEmailUsingTemplateAsync( string from, string to, string template, string? language, IDictionary<string, string> fields);
    
    
}