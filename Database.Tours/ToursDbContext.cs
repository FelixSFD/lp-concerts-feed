using Common.Database.DataObjects;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;

namespace Database.Tours;

/// <summary>
/// DbContext that manages tour data
/// </summary>
/// <param name="options"></param>
public class ToursDbContext(DbContextOptions<ToursDbContext> options) : DbContext(options)
{
    public DbSet<ConcertTypeDo> ConcertTypes { get; set; }
    public DbSet<ConcertDo> Concerts { get; set; }
    
    public DbSet<CountryDo> Countries { get; set; }
    public DbSet<StateDo> States { get; set; }
    public DbSet<CityDo> Cities { get; set; }
    public DbSet<VenueDo> Venues { get; set; }
    public DbSet<PreviousVenueNameDo> PreviousVenueNames { get; set; }
    
    public DbSet<TourDo> Tours { get; set; }
    public DbSet<TourLegDo> TourLegs { get; set; }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<ITimestampedDataObject>();
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CityDo>()
            .HasOne(c => c.State)
            .WithMany()
            .HasForeignKey(c => new { c.CountryCode, c.StateCode })
            .HasPrincipalKey(s => new { s.CountryCode, s.Code })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VenueDo>()
            .HasOne(v => v.City)
            .WithMany()
            .HasForeignKey(v => new { v.CountryCode, v.CityId })
            .HasPrincipalKey(c => new { c.CountryCode, c.Id })
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<VenueDo>()
            .HasOne(v => v.State)
            .WithMany()
            .HasForeignKey(v => new { v.CountryCode, v.StateCode })
            .HasPrincipalKey(s => new { s.CountryCode, s.Code })
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VenueDo>()
            .HasMany(v => v.PreviousNames)
            .WithOne(pn => pn.Venue)
            .HasForeignKey(pn => new { pn.VenueId })
            .HasPrincipalKey(v => new { v.Id });
        
        modelBuilder.Entity<TourLegDo>()
            .HasOne(tl => tl.Tour)
            .WithMany(t => t.Legs)
            .HasForeignKey(tl => new { tl.TourId })
            .HasPrincipalKey(t => new { t.Id });
        
        modelBuilder.Entity<ConcertDo>()
            .HasOne(c => c.TourLeg)
            .WithMany()
            .HasForeignKey(c => new { c.TourLegId })
            .HasPrincipalKey(tl => new { tl.Id })
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<TourDo>()
            .Navigation(t => t.Legs)
            .AutoInclude();
        modelBuilder.Entity<TourLegDo>()
            .Navigation(tl => tl.Tour)
            .AutoInclude();
        
        modelBuilder.Entity<PreviousVenueNameDo>()
            .Property(x => x.From)
            .HasConversion(
                d => d.ToDateTime(TimeOnly.MinValue),
                d => DateOnly.FromDateTime(d));

        modelBuilder.Entity<PreviousVenueNameDo>()
            .Property(x => x.To)
            .HasConversion(
                d => d.HasValue
                    ? d.Value.ToDateTime(TimeOnly.MinValue)
                    : (DateTime?)null,
                d => d.HasValue
                    ? DateOnly.FromDateTime(d.Value)
                    : null);
        
        // Set default value for CreatedAt property of ITimestampedDataObject
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITimestampedDataObject).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ITimestampedDataObject.CreatedAt))
                    .HasDefaultValueSql("NOW()");
            }
        }
        
        // define static data for ConcertTypes
        modelBuilder.Entity<ConcertTypeDo>()
            .HasData(
                new ConcertTypeDo { Id = 1, Name = "Linkin Park Show" },
                new ConcertTypeDo { Id = 2, Name = "Festival" },
                new ConcertTypeDo { Id = 3, Name = "Other" }
                );
    }


    /// <summary>
    /// Makes sure some ConcertTypes exist
    /// </summary>
    public async Task SeedConcertTypes()
    {
        var minimumRequiredTypes = new List<ConcertTypeDo>
        {
            new() { Id = 1, Name = "Linkin Park Show" },
            new() { Id = 2, Name = "Festival" },
            new() { Id = 3, Name = "Other" }
        };

        var existingTypes = await Set<ConcertTypeDo>().ToListAsync();
        var typesToSeed = minimumRequiredTypes.Except(existingTypes).ToList();

        if (typesToSeed.Count != 0)
        {
            await Set<ConcertTypeDo>().AddRangeAsync(typesToSeed);
            await SaveChangesAsync();
        }
    }
}