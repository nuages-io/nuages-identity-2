using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.Storage.SqlServer;



// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class SqlServerPubSubContextFactory : IDesignTimeDbContextFactory<NuagesIdentityDbContext>
{
    public NuagesIdentityDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName!)
            .AddJsonFile("appsettings.sqlserver.json", false)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<NuagesIdentityDbContext>();

        var connectionString =  configuration["ConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("ConnectionString must be provided");

        optionsBuilder
            .UseSqlServer(connectionString, b => b.MigrationsAssembly("Nuages.Identity.Storage.SqlServer"));

        return new NuagesIdentityDbContext(optionsBuilder.Options);
    }
}
