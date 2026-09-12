using LPCalendar.DataStructure.Tours;

namespace Service.Tours.DataStructure;

public class ConcertImportStatusBo
{
    /// <summary>
    /// ID of the page on Linkinpedia
    /// </summary>
    public required string WikiPageId { get; set; }
    
    /// <summary>
    /// If imported, this contains the concert details
    /// </summary>
    public ConcertDetailsBo? Concert { get; set; }

    /// <summary>
    /// Status of the import
    /// </summary>
    public Status ImportStatus { get; set; }


    public enum Status
    {
        Imported,
        ImportedWithSetlists,
        NotImported,
    }
}