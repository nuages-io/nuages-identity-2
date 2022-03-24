namespace Nuages.Identity.Services.Email.Sender;

#nullable disable

public class MessageServiceOptions
{
    public string DefaultCulture { get; set; } = "en";
    public string SendFromEmail { get; set; }
}