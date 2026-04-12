namespace Database.Setlists.DataObjects;

public interface ILinkinpediaLinkable
{
    /// <summary>
    /// Optional link to the wiki page on Linkinpedia
    /// </summary>
    public string? LinkinpediaUrl { get; set; }
}