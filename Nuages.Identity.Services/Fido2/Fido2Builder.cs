namespace Nuages.Identity.Services.Fido2;

public class Fido2Builder : IFido2Builder
{
    public Fido2Builder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

}

public interface IFido2Builder
{
    IServiceCollection Services { get; }
}