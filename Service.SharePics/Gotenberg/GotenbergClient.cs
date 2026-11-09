using System.Text;
using Microsoft.Extensions.Logging;

namespace Service.SharePics.Gotenberg;

public class GotenbergClient(
    HttpClient httpClient,
    ILogger<GotenbergClient> logger
) : IGotenbergClient
{
    public async Task<byte[]> ConvertHtmlToPngAsync(string html, CancellationToken cancellationToken = default)
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