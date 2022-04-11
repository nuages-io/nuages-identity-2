using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Nuages.Identity.UI.Pages;

public class Config : PageModel
{
    private readonly IConfiguration _configuration;

    public Config(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void OnGet()
    {
        Values = _configuration.AsEnumerable();
    }

    public IEnumerable<KeyValuePair<string, string>> Values { get; set; }
}