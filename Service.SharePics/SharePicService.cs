using System.Text;
using Microsoft.Extensions.Logging;
using Service.SharePics.Gotenberg;

namespace Service.SharePics;

public class SharePicService(IGotenbergClient gotenbergClient, ILogger<SharePicService> logger)
{
    public async Task<byte[]> GenerateSharePicAsync(string templateDir, IDictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        var templateFiles = Directory.GetFiles(templateDir, "*", SearchOption.AllDirectories);
        var indexFileName = templateFiles
            .FirstOrDefault(f => Path.GetFileName(f).Equals("index.html", StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException("No index.html file found in template directory", templateDir);
        
        var templateHtml = await File.ReadAllTextAsync(indexFileName, cancellationToken);
        return await gotenbergClient.ConvertHtmlToPngAsync(templateHtml, cancellationToken);
    }
}