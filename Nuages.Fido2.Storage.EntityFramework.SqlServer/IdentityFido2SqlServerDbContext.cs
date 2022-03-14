using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nuages.Fido2.Storage.EntityFramework.SqlServer;

public class IdentityFido2SqlServerDbContext : IdentityFido2DbContext
{
    public IdentityFido2SqlServerDbContext(DbContextOptions context) : base(context)
    {
    }
}


// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class SqlServerPubSubContextFactory : IDesignTimeDbContextFactory<IdentityFido2SqlServerDbContext>
{
    public IdentityFido2SqlServerDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.sqlserver.json", false)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<IdentityFido2DbContext>();

        var connectionString =  configuration["ConnectionString"];
        
        optionsBuilder
            .UseSqlServer(connectionString);

        return new IdentityFido2SqlServerDbContext(optionsBuilder.Options);
    }
}
