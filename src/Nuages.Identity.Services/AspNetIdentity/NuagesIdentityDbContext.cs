using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nuages.Identity.Services.Fido2.AspNetIdentity;

namespace Nuages.Identity.Services.AspNetIdentity;

public class NuagesIdentityDbContext : IdentityDbContext<NuagesApplicationUser<string>,NuagesApplicationRole<string>, string>
{
    public NuagesIdentityDbContext(DbContextOptions<NuagesIdentityDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Fido2Credential> Fido2Credentials { get; set; } = null!;
    
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