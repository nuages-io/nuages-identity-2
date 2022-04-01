using Microsoft.AspNetCore;
using OpenIddict.Abstractions;

namespace Nuages.Identity.UI.OpenIdDict;

public class OpenIddictServerRequestProvider : IOpenIddictServerRequestProvider
{
    private readonly IHttpContextAccessor _contextAccessor;

    public OpenIddictServerRequestProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public OpenIddictRequest? GetOpenIddictServerRequest()
    {
        return _contextAccessor.HttpContext!.GetOpenIddictServerRequest();
    }
}

public interface IOpenIddictServerRequestProvider
{
    OpenIddictRequest? GetOpenIddictServerRequest();
}