using Microsoft.EntityFrameworkCore;

namespace Nuages.Fido2.Storage.EntityFramework;

public class IdentityFido2DbContext : DbContext
{
    public DbSet<Fido2Credential> Fido2Credentials { get; set; } = null!;
    
    public IdentityFido2DbContext(DbContextOptions context) : base(context)
    {
    }

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