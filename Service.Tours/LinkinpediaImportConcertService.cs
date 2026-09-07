using Common.WikiMedia.Repositories;
using Service.Tours.DataStructure;

namespace Service.Tours;

public class LinkinpediaImportConcertService(IWikiMediaRepository wikiMediaRepository)
{
    public async Task<ImportConcertPreviewBo> GetConcertImportPlan(string linkinpediaUrl)
    {
        return null;
    }
}