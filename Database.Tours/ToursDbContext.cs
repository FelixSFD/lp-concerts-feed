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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CityDo>()
            .HasOne(c => c.State)
            .WithMany()
            .HasForeignKey(c => new { c.CountryCode, c.StateCode })
            .HasPrincipalKey(s => new { s.CountryCode, s.Code })
            .IsRequired(false);

        modelBuilder.Entity<VenueDo>()
            .HasOne(v => v.City)
            .WithMany()
            .HasForeignKey(v => new { v.CountryCode, v.CityId })
            .HasPrincipalKey(c => new { c.CountryCode, c.Id });
        
        modelBuilder.Entity<VenueDo>()
            .HasOne(v => v.State)
            .WithMany()
            .HasForeignKey(v => new { v.CountryCode, v.StateCode })
            .HasPrincipalKey(s => new { s.CountryCode, s.Code });

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
            .HasPrincipalKey(tl => new { tl.Id });
        
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
    }
}