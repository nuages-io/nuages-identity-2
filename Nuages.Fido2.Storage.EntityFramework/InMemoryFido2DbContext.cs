using Microsoft.EntityFrameworkCore;

namespace Nuages.Fido2.Storage.EntityFramework;

public class InMemoryFido2DbContext : DbContext
{
    public DbSet<Fido2Credential> Fido2Credentials { get; set; } 
    
    public InMemoryFido2DbContext(DbContextOptions context) : base(context)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Fido2Credential>()
            .HasKey(c => new { c.Id });
        
        modelBuilder.Entity<Fido2Credential>()
            .HasIndex(c => new { c.UserIdBase64 });
        
        
    }
}