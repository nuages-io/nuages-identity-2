using System.Collections;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Nuages.Identity.UI.Pages;

public class DebugEnv : PageModel
{
    public List<Variable> EnvVariables { get; set; } = new List<Variable>();
    
    public void OnGet()
    {
        foreach (DictionaryEntry v in System.Environment.GetEnvironmentVariables())
        {
            EnvVariables.Add(new Variable
            {
                Key = v.Key.ToString(),
                Value = v.Value.ToString()
            });
        }
    }

}

public class Variable
{
    public string Key { get; set; }
    public string Value { get; set; }
}