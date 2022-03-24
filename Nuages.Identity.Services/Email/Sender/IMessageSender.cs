namespace Nuages.Identity.Services.Email.Sender;

public interface IMessageSender
{
    Task<string> SendEmailUsingTemplateAsync(string to, string template, IDictionary<string, string>? fields = null,
        string? language = null, string? from = null);
    
    Task SendSmsAsync(string to, string text);
}

public class MessageSenderToConsole : IMessageSender
{
    private const string Seprator = "----------------------------------------------------------------------";
    
    public Task<string> SendEmailUsingTemplateAsync(string to, string template, IDictionary<string, string>? fields = null, string? language = null,
        string? from = null)
    {
        Console.WriteLine(Seprator);
        Console.WriteLine($"To = {to}");
        Console.WriteLine($"From = {from}");
        Console.WriteLine($"Template = {template}");
        Console.WriteLine($"Language = {language}");

        if (fields != null)
        {
            foreach (var f in fields)
            {
                Console.WriteLine($"{f.Key} = {f.Value}");
            }
        }
        
        Console.WriteLine(Seprator);

        return Task.FromResult(Guid.NewGuid().ToString());

    }

    public Task SendSmsAsync(string to, string text)
    {
        Console.WriteLine(Seprator);
        Console.WriteLine($"To = {to}");
        Console.WriteLine($"Text = {text}");
        Console.WriteLine(Seprator);

        return Task.CompletedTask;
    }
}