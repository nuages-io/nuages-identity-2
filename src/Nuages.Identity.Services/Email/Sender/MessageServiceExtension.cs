using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.Email.Sender;

public static class MessageServiceExtension
{
    public static void AddMessageService(this IdentityBuilder builder, Action<MessageServiceOptions> configure)
    {
        builder.Services.Configure(configure);
        
        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddScoped<IEmailMessageSender, MessageSender>();
        builder.Services.AddScoped<ISmsMessageSender, MessageSender>();
    }
}