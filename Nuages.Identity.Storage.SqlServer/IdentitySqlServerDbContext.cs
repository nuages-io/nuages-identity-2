using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.Storage.SqlServer;

public class IdentitySqlServerDbContext : IdentityDbContext<NuagesApplicationUser<string>,NuagesApplicationRole<string>, string>

{
    public IdentitySqlServerDbContext(DbContextOptions<IdentitySqlServerDbContext> options)
        : base(options)
    {
    }
}