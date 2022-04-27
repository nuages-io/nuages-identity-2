using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Nuages.Identity.Services;

namespace Nuages.Identity.AWS;

// ReSharper disable once InconsistentNaming
public class AwsIdentityEventBus : IIdentityEventBus
{
    private readonly IAmazonEventBridge _eventBridge;
    private readonly ILogger<AwsIdentityEventBus> _eventBus;

    public AwsIdentityEventBus(IAmazonEventBridge eventBridge, ILogger<AwsIdentityEventBus> eventBus)
    {
        _eventBridge = eventBridge;
        _eventBus = eventBus;
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
        
        _eventBus.LogInformation("EVENT BRIDGE => OnLogin :" + res.HttpStatusCode + " failed = " + res.FailedEntryCount);
    }
}