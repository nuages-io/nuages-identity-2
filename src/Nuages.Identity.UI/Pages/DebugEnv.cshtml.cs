using System.Collections;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Nuages.Identity.UI.Pages;

public class DebugEnv : PageModel
{
    private readonly IConfiguration _configuration;

    public DebugEnv(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public List<Variable> EnvVariables { get; set; } = new();
    public List<Variable> Configs { get; set; } = new ();

    public void OnGet()
    {
        // foreach (DictionaryEntry v in System.Environment.GetEnvironmentVariables())
        // {
        //     EnvVariables.Add(new Variable
        //     {
        //         Key = v.Key.ToString(),
        //         Value = v.Value.ToString()
        //     });
        // }
        //
        // foreach (var v in _configuration.AsEnumerable())
        // {
        //     Configs.Add(new Variable
        //     {
        //         Key = v.Key,
        //         Value = v.Value!
        //     });
        // }
    }

}

public class Variable
{
    public string Key { get; set; }
    public string Value { get; set; }
}