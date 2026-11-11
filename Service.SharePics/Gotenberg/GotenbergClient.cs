using System.Text;
using Microsoft.Extensions.Logging;

namespace Service.SharePics.Gotenberg;

public class GotenbergClient(
    HttpClient httpClient,
    ILogger<GotenbergClient> logger
) : IGotenbergClient
{
    public async Task<byte[]> ConvertHtmlToPngAsync(string html, GotenbergResourceFile[] additionalFiles, GotenbergOutputConfig outputConfig, CancellationToken cancellationToken = default)
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
        var width = outputConfig.Width;
        var height = outputConfig.Height;
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
        
        form.Add(new StringContent("true"), "clip");
        
        if (!string.IsNullOrWhiteSpace(outputConfig.ElementSelector))
        {
            form.Add(new StringContent(outputConfig.ElementSelector), "selector");
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