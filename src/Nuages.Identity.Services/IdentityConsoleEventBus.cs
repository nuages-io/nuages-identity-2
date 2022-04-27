using System.Text.Json;

namespace Nuages.Identity.Services;

public class IdentityConsoleEventBus : IIdentityEventBus
{
    private readonly ILogger<IdentityConsoleEventBus> _logger;

    public IdentityConsoleEventBus(ILogger<IdentityConsoleEventBus> logger)
    {
        _logger = logger;
    }
    
 
    public Task PutEvent(IdentityEvents eventName, object detail)
    {
        _logger.LogInformation($"Event : {eventName} " + JsonSerializer.Serialize(detail));
        
        return Task.CompletedTask;
    }
}