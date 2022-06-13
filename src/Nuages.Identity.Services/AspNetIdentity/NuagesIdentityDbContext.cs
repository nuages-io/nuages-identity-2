using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nuages.Identity.Services.Fido2.AspNetIdentity;
using OpenIddict.EntityFrameworkCore.Models;
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.Services.AspNetIdentity;

public class NuagesIdentityDbContext : IdentityDbContext<NuagesApplicationUser<string>,NuagesApplicationRole<string>, string>
{
    public NuagesIdentityDbContext(DbContextOptions<NuagesIdentityDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Fido2Credential> Fido2Credentials { get; set; } = null!;

    // ReSharper disable once UnusedMember.Global
    [ExcludeFromCodeCoverage]
    public DbSet<OpenIddictEntityFrameworkCoreApplication> OpenIddictApplications { get; set; } = null!;
    [ExcludeFromCodeCoverage]
    public DbSet<OpenIddictEntityFrameworkCoreScope> OpenIddictScopes { get; set; } = null!;
    [ExcludeFromCodeCoverage]
    public DbSet<OpenIddictEntityFrameworkCoreAuthorization> OpenIddictAuthorizations { get; set; } = null!;
    [ExcludeFromCodeCoverage]
    public DbSet<OpenIddictEntityFrameworkCoreToken> OpenIddictTokens { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Fido2Credential>()
            .HasKey(c => new { c.Id });
        
        modelBuilder.Entity<Fido2Credential>()
            .HasIndex(c => new { c.UserIdBase64 });
        
        modelBuilder.Entity<Fido2Credential>()
            .HasIndex(c => new { c.UserHandleBase64 });
        
        modelBuilder.Entity<Fido2Credential>()
            .HasIndex(c => new { c.DescriptorIdBase64 }).IsUnique();
         
        modelBuilder.Entity<Fido2Credential>()
            .HasIndex(c => new { c.UserIdBase64, c.DescriptorIdBase64 });
    }
}