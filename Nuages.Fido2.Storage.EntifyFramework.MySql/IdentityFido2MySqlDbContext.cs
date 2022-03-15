
using Microsoft.EntityFrameworkCore;
using Nuages.Fido2.Storage.EntityFramework;

namespace Nuages.Fido2.Storage.EntifyFramework.MySql;

public class IdentityFido2MySqlDbContext : IdentityFido2DbContext

{
    public IdentityFido2MySqlDbContext(DbContextOptions<IdentityFido2MySqlDbContext> options)
        : base(options)
    {
    }
}