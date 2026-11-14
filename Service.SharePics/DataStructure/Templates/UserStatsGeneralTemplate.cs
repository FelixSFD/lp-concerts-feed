namespace Service.SharePics.DataStructure.Templates;

public abstract class UserTemplateBase : SharePicTemplate
{
    /// <summary>
    /// Name of the user
    /// </summary>
    public required string Username { get; set; }
    
    /// <summary>
    /// ID of the user
    /// </summary>
    public required string UserId { get; set; }
}

public class UserTemplate : UserTemplateBase
{
    public override string TemplateId => "user-stats-general";
    public override string MainElementSelector => "#main-card";

    public int Cities { get; set; }
    public int Countries { get; set; }
    public int Songs { get; set; }

    public required Concert[] Concerts { get; set; }

    internal override IDictionary<string, object> GetParameterValues()
    {
        return new Dictionary<string, object>
        {
            { "username", Username },
            { "concerts", Concerts },
            { "cities", Cities },
            { "countries", Countries },
            { "songs", Songs },
        };
    }

    public class Concert
    {
        public required string Location { get; set; }
        public DateTime Date { get; set; }
    }
}