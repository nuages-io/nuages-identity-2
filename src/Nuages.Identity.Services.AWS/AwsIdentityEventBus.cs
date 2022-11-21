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
    private readonly IConfiguration _configuration;

    public AwsIdentityEventBus(IAmazonEventBridge eventBridge, ILogger<AwsIdentityEventBus> logger, IConfiguration configuration)
    {
        _eventBridge = eventBridge;
        _logger = logger;
        _configuration = configuration;
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
                    EventBusName = _configuration["Nuages:EventBus:Name"],
                    Source = _configuration["Nuages:EventBus:Source"]
                }
            }
        });
        
        _logger.LogInformation("EVENT BRIDGE => OnLogin :" + res.HttpStatusCode + " failed = " + res.FailedEntryCount);
    }
}