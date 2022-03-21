using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nuages.Fido2.Storage.EntifyFramework.MySql;



// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class SqlServerPubSubContextFactory : IDesignTimeDbContextFactory<IdentityFido2MySqlDbContext>
{
    public IdentityFido2MySqlDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.mysql2.json", false)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<IdentityFido2MySqlDbContext>();

        var connectionString =  configuration["ConnectionString"];
        
        optionsBuilder
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        
        return new IdentityFido2MySqlDbContext(optionsBuilder.Options);
    }
}
