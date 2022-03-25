using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.Storage.EntityFramework;

public class NuagesIdentityDbContext : IdentityDbContext<NuagesApplicationUser<string>,NuagesApplicationRole<string>, string>
{
    public NuagesIdentityDbContext(DbContextOptions<NuagesIdentityDbContext> options)
        : base(options)
    {
    }
}