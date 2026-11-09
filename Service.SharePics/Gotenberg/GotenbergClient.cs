using System.Text;
using Microsoft.Extensions.Logging;

namespace Service.SharePics.Gotenberg;

public class GotenbergClient(
    HttpClient httpClient,
    ILogger<GotenbergClient> logger
) : IGotenbergClient
{
    public async Task<byte[]> ConvertHtmlToPngAsync(string html, GotenbergResourceFile[] additionalFiles, uint? width = null, uint? height = null, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();

        var htmlContent = new StringContent(
            html,
            Encoding.UTF8,
            "text/html"
        );

        form.Add(htmlContent, "files", "index.html");

        // Add additional files
        foreach (var additionalFile in additionalFiles)
        {
            Log.GotenbergAddAdditionalFile(logger, additionalFile.FileName, additionalFile.FileContent.Length);
            var fileContent = new StreamContent(new MemoryStream(additionalFile.FileContent));
            form.Add(fileContent, "files", additionalFile.FileName);
        }
        
        // add additional parameters
        uint deviceScaleFactor = 2;
        if (width.HasValue)
        {
            width *= deviceScaleFactor;
            form.Add(new StringContent(width.Value.ToString()), "width");
        }

        if (height.HasValue)
        {
            height *= deviceScaleFactor;
            form.Add(new StringContent(height.Value.ToString()), "height");
        }
        
        if (width.HasValue || height.HasValue)
        {
            form.Add(new StringContent("true"), "clip");
        }
        
        form.Add(new StringContent(deviceScaleFactor.ToString()), "deviceScaleFactor");
        
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