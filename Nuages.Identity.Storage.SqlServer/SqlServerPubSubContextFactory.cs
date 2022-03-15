using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace Nuages.Identity.Storage.SqlServer;



// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class SqlServerPubSubContextFactory : IDesignTimeDbContextFactory<IdentitySqlServerDbContext>
{
    public IdentitySqlServerDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.sqlserver.json", false)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<IdentitySqlServerDbContext>();

        var connectionString =  configuration["ConnectionString"];
        
        optionsBuilder
            .UseSqlServer(connectionString);

        return new IdentitySqlServerDbContext(optionsBuilder.Options);
    }
}
