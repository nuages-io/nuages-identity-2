using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    private readonly Logger<ErrorModel> _logger;

    public ErrorModel(Logger<ErrorModel> logger)
    {
        _logger = logger;
    }
    public string RequestId { get; set; } = null!;

    //public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    // ReSharper disable once UnusedMember.Global
    public void OnGet()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (feature != null)
            _logger.LogError(feature.Error, feature.Error.Message);
        
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}