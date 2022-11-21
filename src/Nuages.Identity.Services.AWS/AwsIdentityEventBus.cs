using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Nuages.Identity.Services;

namespace Nuages.Identity.AWS;

// ReSharper disable once InconsistentNaming
public class AwsIdentityEventBus : IIdentityEventBus
{
    private readonly IAmazonEventBridge _eventBridge;
    private readonly ILogger<AwsIdentityEventBus> _logger;

    public AwsIdentityEventBus(IAmazonEventBridge eventBridge, ILogger<AwsIdentityEventBus> logger)
    {
        _eventBridge = eventBridge;
        _logger = logger;
    }
    
    public async Task PutEvent(IdentityEvents eventName, object detail)
    {
        var res = await  _eventBridge.PutEventsAsync(new PutEventsRequest
        {
            Entries = new List<PutEventsRequestEntry>
            {
                new ()
                {
                    Detail = JsonSerializer.Serialize(detail),
                    DetailType = eventName.ToString(),
                    EventBusName = "nuages-identity-events",
                    Source = "nuages.identity"
                }
            }
        });
        
        _logger.LogInformation("EVENT BRIDGE => OnLogin :" + res.HttpStatusCode + " failed = " + res.FailedEntryCount);
    }
}