using Common.Database.DataObjects.Types;

namespace Common.Database.DataObjects;

public interface ILinkinpediaLinkable
{
    /// <summary>
    /// Optional link to the wiki page on Linkinpedia
    /// </summary>
    public LinkinpediaUrl? LinkinpediaUrl { get; set; }
}
