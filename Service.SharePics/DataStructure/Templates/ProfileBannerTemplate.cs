namespace Service.SharePics.DataStructure.Templates;

public class ProfileBannerTemplate : UserTemplateBase
{
    public override string TemplateId => "user-banner";
    public override string MainElementSelector => "body";

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