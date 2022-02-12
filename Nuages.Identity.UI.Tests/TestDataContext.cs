using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Nuages.AspNetIdentity.Core;

namespace Nuages.Identity.UI.Tests;

public class TestDataContext : IdentityDbContext<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>
{
    public TestDataContext(DbContextOptions<TestDataContext> context) : base(context)
    {
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<NuagesApplicationUser<string>>()
            .Property(b => b.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<StringGuidValueGenerator>();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class StringGuidValueGenerator : ValueGenerator<string>
{
    /// <summary>
    ///     Gets a value indicating whether the values generated are temporary or permanent. This implementation
    ///     always returns false, meaning the generated values will be saved to the database.
    /// </summary>
    public override bool GeneratesTemporaryValues
        => false;

    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <returns>The value to be assigned to a property.</returns>
    public override string Next(EntityEntry entry)
    {
        return Guid.NewGuid().ToString();
    }
}