using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.Storage.MySql;

public class IdentityMySqlDbContext : IdentityDbContext<NuagesApplicationUser<string>,NuagesApplicationRole<string>, string>

{
    public IdentityMySqlDbContext(DbContextOptions<IdentityMySqlDbContext> options)
        : base(options)
    {
    }
}