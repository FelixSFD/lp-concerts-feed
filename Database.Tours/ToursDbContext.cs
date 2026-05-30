using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

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
            .HasPrincipalKey(s => new { s.CountryCode, s.Code });

        modelBuilder.Entity<VenueDo>()
            .HasOne(v => v.City)
            .WithMany()
            .HasForeignKey(v => new { v.CountryCode, v.StateCode, v.CityId })
            .HasPrincipalKey(c => new { c.CountryCode, c.StateCode, c.Id });
        
        modelBuilder.Entity<VenueDo>()
            .HasOne(v => v.State)
            .WithMany()
            .HasForeignKey(v => new { v.CountryCode, v.StateCode })
            .HasPrincipalKey(s => new { s.CountryCode, s.Code });
        
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
        
        modelBuilder.Entity<TourLegDo>()
            .Navigation(tl => tl.Tour)
            .AutoInclude();
    }
}