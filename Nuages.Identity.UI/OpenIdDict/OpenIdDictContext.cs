using Microsoft.EntityFrameworkCore;

namespace Nuages.Identity.UI.OpenIdDict;

public class OpenIdDictContext: DbContext
{

    public OpenIdDictContext(DbContextOptions<OpenIdDictContext> options) : base (options)
    {
        
    }
}