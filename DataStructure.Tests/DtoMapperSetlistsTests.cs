using Common.Contracts.Generated.Models;
using LPCalendar.DataStructure.Setlists;
using LPCalendar.DataStructure.Setlists.Import;
using LPCalendar.DataStructure.Setlists.Parameters;
using Server.Api;
using Xunit;

namespace DataStructure.Tests;

public class DtoMapperSetlistsTests
{
    [Fact]
    public void TestAlbumMapping()
    {
        var bo = new AlbumBo
        {
            Id = 42,
            Title = "Hybrid Theory",
            LinkinpediaUrl = "https://linkinpedia.com/Hybrid_Theory"
        };

        var dto = bo.ToDto();
        Assert.Equal((decimal)42, dto.Id);
        Assert.Equal("Hybrid Theory", dto.Title);
        Assert.Equal("https://linkinpedia.com/Hybrid_Theory", dto.LinkinpediaUrl);

        var mappedBo = dto.ToBo();
        Assert.Equal(bo.Id, mappedBo.Id);
        Assert.Equal(bo.Title, mappedBo.Title);
        Assert.Equal(bo.LinkinpediaUrl, mappedBo.LinkinpediaUrl);

        var createReqDto = new CreateAlbumRequestDto { Title = "Meteora", LinkinpediaUrl = "https://linkinpedia.com/Meteora" };
        var createReqBo = createReqDto.ToBo();
        Assert.Equal("Meteora", createReqBo.Title);
        Assert.Equal(createReqDto.Title, createReqBo.ToDto().Title);

        var updateReqDto = new UpdateAlbumRequestDto { Title = "Minutes to Midnight", LinkinpediaUrl = "https://linkinpedia.com/MTM" };
        var updateReqBo = updateReqDto.ToBo();
        Assert.Equal("Minutes to Midnight", updateReqBo.Title);
        Assert.Equal(updateReqDto.Title, updateReqBo.ToDto().Title);
    }

    [Fact]
    public void TestSongMapping()
    {
        var bo = new SongBo
        {
            Id = 10,
            Title = "In The End",
            Album = new AlbumBo { Id = 1, Title = "Hybrid Theory" },
            Isrc = "USWB10000001",
            AppleMusicId = "123456",
            LinkinpediaUrl = "https://linkinpedia.com/In_The_End"
        };

        var dto = bo.ToDto();
        Assert.Equal((decimal)10, dto.Id);
        Assert.Equal("In The End", dto.Title);
        Assert.NotNull(dto.Album);
        Assert.Equal((decimal)1, dto.Album.Id);

        var mappedBo = dto.ToBo();
        Assert.Equal(bo.Id, mappedBo.Id);
        Assert.Equal(bo.Title, mappedBo.Title);
        Assert.NotNull(mappedBo.Album);
        Assert.Equal(bo.Album.Id, mappedBo.Album.Id);

        var createReqDto = new CreateSongRequestDto { Title = "Crawling", AlbumId = 1, Isrc = "USWB10000002" };
        var createReqBo = createReqDto.ToBo();
        Assert.Equal("Crawling", createReqBo.Title);
        Assert.Equal((uint)1, createReqBo.AlbumId);
        Assert.Equal(createReqDto.Title, createReqBo.ToDto().Title);

        var updateReqDto = new UpdateSongRequestDto { Title = "Numb", AlbumId = 2, Isrc = "USWB10000003" };
        var updateReqBo = updateReqDto.ToBo();
        Assert.Equal("Numb", updateReqBo.Title);
        Assert.Equal(updateReqDto.Title, updateReqBo.ToDto().Title);
    }

    [Fact]
    public void TestSongVariantMapping()
    {
        var bo = new SongVariantBo
        {
            Id = 5,
            SongId = 10,
            VariantName = "2024 Version",
            Description = "Live performance version",
            IsrcOverride = "USWB10000099",
            AppleMusicIdOverride = "987654"
        };

        var dto = bo.ToDto();
        Assert.Equal((decimal)5, dto.Id);
        Assert.Equal((decimal)10, dto.SongId);
        Assert.Equal("2024 Version", dto.VariantName);

        var mappedBo = dto.ToBo();
        Assert.Equal(bo.Id, mappedBo.Id);
        Assert.Equal(bo.SongId, mappedBo.SongId);
        Assert.Equal(bo.VariantName, mappedBo.VariantName);
    }

    [Fact]
    public void TestSongMashupMapping()
    {
        var bo = new SongMashupBo
        {
            Id = 3,
            Title = "Numb / Encore",
            LinkinpediaUrl = "https://linkinpedia.com/Numb_Encore",
            Songs = [new SongBo { Id = 1, Title = "Numb" }]
        };

        var dto = bo.ToDto();
        Assert.Equal((decimal)3, dto.Id);
        Assert.Equal("Numb / Encore", dto.Title);
        Assert.Single(dto.Songs);

        var mappedBo = dto.ToBo();
        Assert.Equal(bo.Id, mappedBo.Id);
        Assert.Equal(bo.Title, mappedBo.Title);
        Assert.Single(mappedBo.Songs);

        var createReqDto = new CreateSongMashupRequestDto { Title = "Dirt Off Your Shoulder / Lying From You", SongIds = [1, 2] };
        var createReqBo = createReqDto.ToBo();
        Assert.Equal(createReqDto.Title, createReqBo.Title);
        Assert.Equal(2, createReqBo.SongIds.Length);
        Assert.Equal(createReqDto.Title, createReqBo.ToDto().Title);

        var updateReqDto = new UpdateSongMashupRequestDto { Title = "Jigga What / Faint", SongIds = [3, 4] };
        var updateReqBo = updateReqDto.ToBo();
        Assert.Equal(updateReqDto.Title, updateReqBo.Title);
        Assert.Equal(2, updateReqBo.SongIds.Length);
        Assert.Equal(updateReqDto.Title, updateReqBo.ToDto().Title);
    }

    [Fact]
    public void TestParametersMapping()
    {
        var actParamsBo = new ActParametersBo { SetlistId = 1, ActNumber = 2, Title = "Encore" };
        var actParamsDto = actParamsBo.ToDto();
        Assert.Equal(1L, actParamsDto.SetlistId);
        Assert.Equal(2L, actParamsDto.ActNumber);
        Assert.Equal("Encore", actParamsDto.Title);
        var actMappedBo = actParamsDto.ToBo();
        Assert.Equal(actParamsBo.SetlistId, actMappedBo.SetlistId);
        Assert.Equal(actParamsBo.ActNumber, actMappedBo.ActNumber);

        var entryParamsBo = new SetlistEntryParametersBo
        {
            SortNumber = 10,
            SongNumber = 1,
            TitleOverride = "Custom Title",
            ExtraNotes = "Special guest",
            IsRotationSong = true,
            IsPlayedFromRecording = false,
            IsWorldPremiere = true,
            IsLivePremiere = false
        };
        var entryParamsDto = entryParamsBo.ToDto();
        Assert.Equal((decimal)10, entryParamsDto.SortNumber);
        Assert.Equal((decimal)1, entryParamsDto.SongNumber);
        Assert.Equal("Custom Title", entryParamsDto.TitleOverride);
        Assert.True(entryParamsDto.IsRotationSong);
        var entryMappedBo = entryParamsDto.ToBo();
        Assert.Equal(entryParamsBo.SortNumber, entryMappedBo.SortNumber);
        Assert.Equal(entryParamsBo.SongNumber, entryMappedBo.SongNumber);
        Assert.Equal(entryParamsBo.TitleOverride, entryMappedBo.TitleOverride);

        var songParamsBo = new SongParametersBo { SongId = 15, SongTitle = "Somewhere I Belong", Isrc = "USWB123", AppleMusicId = "789" };
        var songParamsDto = songParamsBo.ToDto();
        Assert.Equal((decimal)15, songParamsDto.Id);
        Assert.Equal("Somewhere I Belong", songParamsDto.Title);
        var songMappedBo = songParamsDto.ToBo();
        Assert.Equal(songParamsBo.SongId, songMappedBo.SongId);
        Assert.Equal(songParamsBo.SongTitle, songMappedBo.SongTitle);

        var variantParamsBo = new SongVariantParametersBo { SongVariantId = 8, SongId = 15, VariantName = "Ext Intro", Description = "Extended intro" };
        var variantParamsDto = variantParamsBo.ToDto();
        Assert.Equal((decimal)8, variantParamsDto.Id);
        Assert.Equal((decimal)15, variantParamsDto.SongId);
        var variantMappedBo = variantParamsDto.ToBo();
        Assert.Equal(variantParamsBo.SongVariantId, variantMappedBo.SongVariantId);
        Assert.Equal(variantParamsBo.VariantName, variantMappedBo.VariantName);

        var mashupParamsBo = new SongMashupParametersBo { SongMashupId = 9 };
        var mashupParamsDto = mashupParamsBo.ToDto();
        Assert.Equal((decimal)9, mashupParamsDto.Id);
        var mashupMappedBo = mashupParamsDto.ToBo();
        Assert.Equal(mashupParamsBo.SongMashupId, mashupMappedBo.SongMashupId);
    }

    [Fact]
    public void TestSetlistAndEntryMapping()
    {
        var extraBo = new SetlistEntrySongExtraBo
        {
            Id = "extra-1",
            Type = SetlistEntrySongExtraBo.ExtraType.ExtendedBridge,
            Description = "Bridge singalong",
            Song = new SongBo { Id = 1, Title = "Bleed It Out" }
        };
        var extraDto = extraBo.ToDto();
        Assert.Equal(SetlistEntrySongExtraDto.TypeEnum.ExtendedBridge, extraDto.Type);
        Assert.Equal("Bridge singalong", extraDto.Description);
        var extraMappedBo = extraDto.ToBo();
        Assert.Equal(extraBo.Type, extraMappedBo.Type);
        Assert.Equal(extraBo.Description, extraMappedBo.Description);

        var entryBo = new SetlistEntryBo
        {
            Id = "entry-1",
            ActNumber = 1,
            SortNumber = 10,
            SongNumber = 1,
            Title = "Papercut",
            PlayedSong = new SongBo { Id = 2, Title = "Papercut" },
            SongExtras = [extraBo],
            IsLivePremiere = true
        };
        var entryDto = entryBo.ToDto();
        Assert.Equal("entry-1", entryDto.Id);
        Assert.Equal("Papercut", entryDto.Title);
        Assert.Single(entryDto.Extras);
        var entryMappedBo = entryDto.ToBo();
        Assert.Equal(entryBo.Id, entryMappedBo.Id);
        Assert.Equal(entryBo.Title, entryMappedBo.Title);
        Assert.Single(entryMappedBo.SongExtras);

        var setlistBo = new SetlistBo
        {
            Id = 100,
            ConcertId = "concert-123",
            ConcertTitle = "London 2024",
            SetName = "Main Set",
            LinkinpediaUrl = "https://linkinpedia.com/2024.09.24",
            Acts = [new SetlistActBo { SetlistId = 100, ActNumber = 1, Title = "Act A" }],
            Entries = [entryBo]
        };
        var setlistDto = setlistBo.ToDto();
        Assert.Equal((decimal)100, setlistDto.Id);
        Assert.Equal("concert-123", setlistDto.ConcertId);
        Assert.Single(setlistDto.Acts);
        Assert.Single(setlistDto.Entries);
        var setlistMappedBo = setlistDto.ToBo();
        Assert.Equal(setlistBo.Id, setlistMappedBo.Id);
        Assert.Equal(setlistBo.ConcertId, setlistMappedBo.ConcertId);
        Assert.Single(setlistMappedBo.Acts);
        Assert.Single(setlistMappedBo.Entries);
    }

    [Fact]
    public void TestImportMapping()
    {
        var previewBo = new ImportSetlistPreviewBo
        {
            ConcertId = "concert-999",
            Acts = [new ImportSetlistActPreviewBo { Name = "Act 1", ActNumber = 1, ExtraNotes = "Notes" }],
            Entries =
            [
                new ImportSetlistEntryPreviewBo
                {
                    Title = "Faint",
                    SongNumber = 1,
                    ActNumber = 1,
                    FoundSongId = 12,
                    FoundSongIds = [12, 13],
                    PossibleSongVariants = [new SongVariantBo { Id = 1, SongId = 12, VariantName = "Extended Outro" }]
                }
            ]
        };

        var previewDto = previewBo.ToDto();
        Assert.Equal("concert-999", previewDto.ConcertId);
        Assert.Single(previewDto.Acts);
        Assert.Single(previewDto.Entries);
        Assert.Equal("Faint", previewDto.Entries[0].Title);

        var previewMappedBo = previewDto.ToBo();
        Assert.Equal(previewBo.ConcertId, previewMappedBo.ConcertId);
        Assert.Single(previewMappedBo.Acts);
        Assert.Single(previewMappedBo.Entries);
        Assert.Equal("Faint", previewMappedBo.Entries[0].Title);

        var importReqDto = new ImportSetlistRequestDto { ConcertId = "concert-999", LinkinpediaUrl = "https://linkinpedia.com/page" };
        var importReqBo = importReqDto.ToBo();
        Assert.Equal("concert-999", importReqBo.ConcertId);
        Assert.Equal("https://linkinpedia.com/page", importReqBo.LinkinpediaUrl);
        Assert.Equal(importReqDto.ConcertId, importReqBo.ToDto().ConcertId);
    }
}
