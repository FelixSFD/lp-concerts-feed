using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists;

public class SetlistsDbContext(DbContextOptions<SetlistsDbContext> options) : DbContext(options)
{
    public DbSet<AlbumDo> Albums { get; set; }
}