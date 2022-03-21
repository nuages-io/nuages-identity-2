using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nuages.Fido2.Storage.EntityFramework.SqlServer;

[ExcludeFromCodeCoverage]
// ReSharper disable once UnusedType.Global
public class SqlServerPubSubContextFactory : IDesignTimeDbContextFactory<IdentityFido2SqlServerDbContext>
{
    public IdentityFido2SqlServerDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.sqlserver2.json", false)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<IdentityFido2SqlServerDbContext>();

        var connectionString =  configuration["ConnectionString"];
        
        optionsBuilder
            .UseSqlServer(connectionString);

        return new IdentityFido2SqlServerDbContext(optionsBuilder.Options);
    }
}