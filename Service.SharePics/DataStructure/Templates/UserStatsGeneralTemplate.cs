namespace Service.SharePics.DataStructure.Templates;

public class UserTemplate : UserTemplateBase
{
    public override string TemplateId => "user-stats-general";
    public override string MainElementSelector => "#main-card";

    public int Cities { get; set; }
    public int Countries { get; set; }
    public int Songs { get; set; }

    public required Concert[] Concerts { get; set; }

    /// <summary>
    /// Rare song that the user has seen live
    /// </summary>
    public RareSong[] RareSongs { get; set; } = [];

    internal override IDictionary<string, object> GetParameterValues()
    {
        return new Dictionary<string, object>
        {
            { "username", Username },
            { "concerts", Concerts },
            { "cities", Cities },
            { "countries", Countries },
            { "songs", Songs },
            { "rareSongs", RareSongs },
        };
    }

    public class Concert
    {
        public required string Location { get; set; }
        public DateTime Date { get; set; }
    }

    public class RareSong
    {
        /// <summary>
        /// Title of the song
        /// </summary>
        public required string Title { get; set; }
        
        /// <summary>
        /// Number of times the user has seen this song live
        /// </summary>
        public uint UserPlayCount { get; set; }
        
        /// <summary>
        /// Total number of times this song has been played live overall
        /// </summary>
        public uint TotalPlayCount { get; set; }
    }
}