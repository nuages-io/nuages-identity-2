namespace Nuages.Identity.Services;

public class CurrentBaseUrlProvider : ICurrentBaseUrlProvider
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IWebHostEnvironment _hostEnvironment;

    public CurrentBaseUrlProvider(IHttpContextAccessor contextAccessor, IWebHostEnvironment hostEnvironment)
    {
        _contextAccessor = contextAccessor;
        _hostEnvironment = hostEnvironment;
    }

    public string GetUrl()
    {
        var scheme = _contextAccessor.HttpContext!.Request.Scheme;
        var host = _contextAccessor.HttpContext.Request.Host.Host;
        if (_hostEnvironment.IsDevelopment())
            host += ":" + _contextAccessor.HttpContext!.Request.Host.Port;
        
        return $"{scheme}://{host}";
    }
}

public interface ICurrentBaseUrlProvider
{
    string GetUrl();
}