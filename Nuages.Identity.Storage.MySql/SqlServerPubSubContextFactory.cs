using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nuages.Identity.Storage.MySql;



// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class SqlServerPubSubContextFactory : IDesignTimeDbContextFactory<IdentityMySqlDbContext>
{
    public IdentityMySqlDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.mysql.json", false)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<IdentityMySqlDbContext>();

        var connectionString =  configuration["ConnectionString"];
        
        optionsBuilder
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        
        return new IdentityMySqlDbContext(optionsBuilder.Options);
    }
}
