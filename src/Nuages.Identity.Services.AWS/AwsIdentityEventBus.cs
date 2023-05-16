using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services;

namespace Nuages.Identity.AWS;

// ReSharper disable once InconsistentNaming
public class AwsIdentityEventBus : IIdentityEventBus
{
    private readonly IAmazonEventBridge _eventBridge;
    private readonly ILogger<AwsIdentityEventBus> _logger;
    private readonly IdentityAWSExtension.EventBusOptions _eventBuOptions;

    public AwsIdentityEventBus(IAmazonEventBridge eventBridge, ILogger<AwsIdentityEventBus> logger, IOptions<IdentityAWSExtension.EventBusOptions> eventBuOptions)
    {
        _eventBridge = eventBridge;
        _logger = logger;
        _eventBuOptions = eventBuOptions.Value;
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
                    EventBusName = _eventBuOptions.Name,
                    Source =_eventBuOptions.Source
                }
            }
        });

        _logger.LogInformation("EVENT BRIDGE => OnLogin :{HttpStatusCode} failed = {Count}", res.HttpStatusCode,res.FailedEntryCount);
    }
}