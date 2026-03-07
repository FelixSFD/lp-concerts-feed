using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists;

public class SetlistsDbContext(DbContextOptions<SetlistsDbContext> options) : DbContext(options)
{
    public DbSet<AlbumDo> Albums { get; set; }
    public DbSet<SongDo> Songs { get; set; }
    public DbSet<SongMashupDo> SongMashups { get; set; }
    public DbSet<SongVariantDo> SongVariants { get; set; }
    public DbSet<SetlistDo> Setlists { get; set; }
    public DbSet<SetlistEntryDo> SetlistEntries { get; set; }
    public DbSet<SetlistActDo> SetlistActs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SetlistEntryDo>()
            .HasOne(e => e.Act)
            .WithMany(a => a.Entries)
            .HasForeignKey(e => new { e.SetlistId, e.ActNumber })
            .HasPrincipalKey(a => new { a.SetlistId, ActNumber = (uint?)a.ActNumber })
            .IsRequired(false); // because ActNumber is nullable
        
        modelBuilder.Entity<SetlistEntryDo>()
            .HasMany(e => e.SongExtras)
            .WithOne(extra => extra.SetlistEntry)
            .IsRequired(false);

        modelBuilder.Entity<SongMashupDo>()
            .HasMany(m => m.Songs)
            .WithMany();
    }
}