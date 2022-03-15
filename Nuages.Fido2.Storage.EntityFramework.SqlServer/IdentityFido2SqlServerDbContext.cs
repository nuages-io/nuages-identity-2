using Microsoft.EntityFrameworkCore;

namespace Nuages.Fido2.Storage.EntityFramework.SqlServer;

public class IdentityFido2SqlServerDbContext : IdentityFido2DbContext
{
    public IdentityFido2SqlServerDbContext(DbContextOptions<IdentityFido2SqlServerDbContext> context) : base(context)
    {
    }
}


// ReSharper disable once UnusedType.Global