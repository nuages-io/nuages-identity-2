namespace Nuages.Identity.Services.Email.Sender;

public class MessageSender : IEmailMessageSender, ISmsMessageSender
{
    private const string Seprator = "----------------------------------------------------------------------";

    public virtual Task<string> SendEmailUsingTemplateAsync(string from, string to, string template, string? language,
        IDictionary<string, string> fields)
    {
        Console.WriteLine(Seprator);
        Console.WriteLine("Implement IEmailMessageSender to send real email. See AWSSender in project Nuages.Identity.UI");
        Console.WriteLine(Seprator);
        Console.WriteLine($"To = {to}");
        Console.WriteLine($"From = {from}");
        Console.WriteLine($"Template = {template}");
        Console.WriteLine($"Language = {language}");

        foreach (var f in fields)
        {
            Console.WriteLine($"{f.Key} = {f.Value}");
        }
        
        Console.WriteLine(Seprator);

        return Task.FromResult(Guid.NewGuid().ToString());

    }

    public virtual Task<string> SendSmsAsync(string to, string text)
    {
        Console.WriteLine(Seprator);
        Console.WriteLine("Implement ISmsMessageSender to send real SMS. See AWSSender in project Nuages.Identity.UI");
        Console.WriteLine(Seprator);
        Console.WriteLine($"To = {to}");
        Console.WriteLine($"Text = {text}");
        Console.WriteLine(Seprator);

        return Task.FromResult(Guid.NewGuid().ToString());
    }
}