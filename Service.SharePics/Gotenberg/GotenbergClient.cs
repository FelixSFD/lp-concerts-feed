using System.Text;
using Microsoft.Extensions.Logging;

namespace Service.SharePics.Gotenberg;

public class GotenbergClient(
    HttpClient httpClient,
    ILogger<GotenbergClient> logger
) : IGotenbergClient
{
    public async Task<byte[]> ConvertHtmlToPngAsync(string html, GotenbergResourceFile[] additionalFiles, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();

        var htmlContent = new StringContent(
            html,
            Encoding.UTF8,
            "text/html"
        );

        form.Add(htmlContent, "files", "index.html");

        foreach (var additionalFile in additionalFiles)
        {
            Log.GotenbergAddAdditionalFile(logger, additionalFile.FileName, additionalFile.FileContent.Length);
            var fileContent = new StreamContent(new MemoryStream(additionalFile.FileContent));
            form.Add(fileContent, "files", additionalFile.FileName);
        }
        
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