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
            .HasOne(e => e.Setlist)
            .WithMany(s => s.Entries)
            .HasForeignKey(e => new { e.SetlistId })
            .HasPrincipalKey(s => new { s.Id });
        
        modelBuilder.Entity<SetlistEntryDo>()
            .HasMany(e => e.SongExtras)
            .WithOne(extra => extra.SetlistEntry)
            .IsRequired(false);
        
        modelBuilder.Entity<SetlistEntryDo>()
            .Navigation(e => e.SongExtras)
            .AutoInclude();

        modelBuilder.Entity<SetlistEntryDo>()
            .Navigation(e => e.PlayedSong)
            .AutoInclude();
        
        modelBuilder.Entity<SetlistEntryDo>()
            .Navigation(e => e.PlayedSongVariant)
            .AutoInclude();

        modelBuilder.Entity<SetlistEntrySongExtraDo>()
            .Navigation(e => e.Song)
            .AutoInclude();
        
        modelBuilder.Entity<SongDo>()
            .Navigation(s => s.Album)
            .AutoInclude();
        modelBuilder.Entity<SongDo>()
            .Navigation(s => s.Variants)
            .AutoInclude();
        
        modelBuilder.Entity<SongVariantDo>()
            .HasOne(e => e.Song)
            .WithMany(s => s.Variants)
            .HasForeignKey(e => new { e.SongId })
            .HasPrincipalKey(s => new { s.Id });
        
        modelBuilder.Entity<SongVariantDo>()
            .Navigation(v => v.Song)
            .AutoInclude();
        
        modelBuilder.Entity<SetlistEntryDo>()
            .Navigation(e => e.PlayedMashup)
            .AutoInclude();
        
        modelBuilder.Entity<SongInMashupDo>(entity =>
        {
            entity.ToTable("SongInMashup");

            entity.HasKey(x => new { x.MashupId, x.SongId });

            entity.HasOne(x => x.Mashup)
                .WithMany(m => m.Songs)
                .HasForeignKey(x => x.MashupId);

            entity.HasOne(x => x.Song)
                .WithMany()
                .HasForeignKey(x => x.SongId);
        });
        
        modelBuilder.Entity<SongMashupDo>()
            .Navigation(e => e.Songs)
            .AutoInclude();
        
        modelBuilder.Entity<SongInMashupDo>()
            .Navigation(e => e.Song)
            .AutoInclude();
        
        modelBuilder.Entity<SongInMashupDo>()
            .Navigation(e => e.Mashup)
            .AutoInclude();
    }
}