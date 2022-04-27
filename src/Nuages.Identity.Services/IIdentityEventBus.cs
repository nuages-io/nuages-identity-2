namespace Nuages.Identity.Services;

public interface IIdentityEventBus
{
    Task PutEvent(IdentityEvents eventName, object detail);
}