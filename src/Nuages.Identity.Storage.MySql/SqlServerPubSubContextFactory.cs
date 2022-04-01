using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.Storage.MySql;



// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class SqlServerPubSubContextFactory : IDesignTimeDbContextFactory<NuagesIdentityDbContext>
{
    public NuagesIdentityDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.mysql.json", false)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<NuagesIdentityDbContext>();

        var connectionString =  configuration["ConnectionString"];
        
        optionsBuilder
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), b => b.MigrationsAssembly("Nuages.Identity.Storage.MySql"));
        
        return new NuagesIdentityDbContext(optionsBuilder.Options);
    }
}
