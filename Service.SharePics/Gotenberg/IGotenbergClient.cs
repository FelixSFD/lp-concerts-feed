namespace Service.SharePics.Gotenberg;

public interface IGotenbergClient
{
    /// <summary>
    /// Uses Gotenberg to convert the given HTML to a PNG image.
    /// The HTML is sent as a multipart/form-data request to the Gotenberg API, which returns the generated PNG image as a byte array.
    /// </summary>
    /// <param name="html"></param>
    /// <param name="additionalFiles"></param>
    /// <param name="outputConfig">Configures how the screenshot will be taken</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> ConvertHtmlToPngAsync(string html, GotenbergResourceFile[] additionalFiles, GotenbergOutputConfig outputConfig, CancellationToken cancellationToken = default);
}