namespace Nuages.Identity.Services.Email.Sender;

public class MessageSender : IEmailMessageSender, ISmsMessageSender
{
    private readonly string _seprator = new string('-', 100);

    public virtual Task<string> SendEmailUsingTemplateAsync(string from, string to, string template, string? language,
        IDictionary<string, string> fields)
    {
        Console.WriteLine(_seprator);
        Console.WriteLine("Implement IEmailMessageSender to send real email. See AWSSender in project Nuages.Identity.UI");
        Console.WriteLine(_seprator);
        Console.WriteLine($"To = {to}");
        Console.WriteLine($"From = {from}");
        Console.WriteLine($"Template = {template}");
        Console.WriteLine($"Language = {language}");

        foreach (var (key, value) in fields)
        {
            Console.WriteLine($"{key} = {value}");
        }
        
        Console.WriteLine(_seprator);

        return Task.FromResult(Guid.NewGuid().ToString());

    }

    public virtual Task<string> SendSmsAsync(string to, string text)
    {
        Console.WriteLine(_seprator);
        Console.WriteLine("Implement ISmsMessageSender to send real SMS. See AWSSender in project Nuages.Identity.UI");
        Console.WriteLine(_seprator);
        Console.WriteLine($"To = {to}");
        Console.WriteLine($"Text = {text}");
        Console.WriteLine(_seprator);

        return Task.FromResult(Guid.NewGuid().ToString());
    }
}