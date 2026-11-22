using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;

public class SqlSetlistActRepositoryTest : SetlistsDbIntegrationTestsBase
{
    [Fact]
    public async Task Update_PersistsTitleForOnlyTheSpecifiedAct()
    {
        var setlists = new SqlSetlistRepository(DbContext);
        var acts = new SqlSetlistActRepository(DbContext);
        var firstSetlist = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(), ConcertTitle = "First", ConcertType = "Concert", ConcertDate = DateTime.Today
        };
        var secondSetlist = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(), ConcertTitle = "Second", ConcertType = "Concert", ConcertDate = DateTime.Today
        };
        setlists.Add(firstSetlist);
        setlists.Add(secondSetlist);
        await setlists.SaveChangesAsync();

        acts.Add(new SetlistActDo { SetlistId = firstSetlist.Id, ActNumber = 1, Title = "Original" });
        acts.Add(new SetlistActDo { SetlistId = firstSetlist.Id, ActNumber = 2, Title = "Other act" });
        acts.Add(new SetlistActDo { SetlistId = secondSetlist.Id, ActNumber = 1, Title = "Other setlist" });
        await acts.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var act = await acts.GetBy(firstSetlist.Id, 1);
        Assert.NotNull(act);
        act.Title = "Updated";
        acts.Update(act);
        await acts.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var updated = await acts.GetBy(firstSetlist.Id, 1);
        Assert.NotNull(updated);
        Assert.Equal(firstSetlist.Id, updated.SetlistId);
        Assert.Equal(1u, updated.ActNumber);
        Assert.Equal("Updated", updated.Title);
        Assert.Equal("Other act", (await acts.GetBy(firstSetlist.Id, 2))?.Title);
        Assert.Equal("Other setlist", (await acts.GetBy(secondSetlist.Id, 1))?.Title);
        Assert.Null(await acts.GetBy(secondSetlist.Id, 2));
    }
}
