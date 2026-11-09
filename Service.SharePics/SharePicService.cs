using System.Text;
using Microsoft.Extensions.Logging;

namespace Service.SharePics;

public class SharePicService(HttpClient httpClient, ILogger<SharePicService> logger)
{
    public async Task<byte[]> GenerateSharePicAsync(string templateDir, IDictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        var templateFiles = Directory.GetFiles(templateDir, "*", SearchOption.AllDirectories);
        var indexFileName = templateFiles
            .FirstOrDefault(f => Path.GetFileName(f).Equals("index.html", StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException("No index.html file found in template directory", templateDir);
        
        var templateHtml = await File.ReadAllTextAsync(indexFileName, cancellationToken);
        return await ConvertHtmlToPngAsync(templateHtml, cancellationToken);
    }
    
    
    /// <summary>
    /// Uses Gotenberg to convert the given HTML to a PNG image.
    /// The HTML is sent as a multipart/form-data request to the Gotenberg API, which returns the generated PNG image as a byte array.
    /// </summary>
    /// <param name="html"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<byte[]> ConvertHtmlToPngAsync(
        string html,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();

        var htmlContent = new StringContent(
            html,
            Encoding.UTF8,
            "text/html"
        );

        form.Add(htmlContent, "files", "index.html");
        
        Log.SendingHtmlToGotenberg(logger, html);

        using var response = await httpClient.PostAsync(
            "/forms/chromium/screenshot/html",
            form,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();
        
        Log.GotenbergConversionSucceeded(logger);

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}