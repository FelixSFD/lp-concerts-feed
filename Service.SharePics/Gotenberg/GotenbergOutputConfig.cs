namespace Service.SharePics.Gotenberg;

/// <summary>
/// Configures the output of the Gotenberg screenshot generation.
/// </summary>
public class GotenbergOutputConfig
{
    /// <summary>
    /// Width of the result in pixels. If not specified, Gotenberg will use a default value
    /// </summary>
    public uint? Width { get; set; }
    
    /// <summary>
    /// Height of the result in pixels. If not specified, Gotenberg will use a default value.
    /// Can be omitted, if <see cref="ElementSelector"/> is used to select a specific element to screenshot.
    /// </summary>
    public uint? Height { get; set; }

    /// <summary>
    /// CSS selector of the element to screenshot. If not specified, the whole page will be screenshotted.
    /// Default is "body", which means the whole page will be screenshotted.
    /// </summary>
    public string ElementSelector { get; set; } = "body";
}