namespace Service.SharePics.DataStructure;

public abstract class SharePicTemplate
{
    /// <summary>
    /// ID of the template
    /// </summary>
    public abstract string TemplateId { get; }

    /// <summary>
    /// Returns the values of the parameters passed to the templating engine
    /// </summary>
    /// <returns>Parameter values</returns>
    internal abstract IDictionary<string, object> GetParameterValues();
}