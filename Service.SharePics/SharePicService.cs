using Microsoft.Extensions.Logging;
using Scriban;
using Service.SharePics.Gotenberg;

namespace Service.SharePics;

public class SharePicService(IGotenbergClient gotenbergClient, string templateBaseDir, ILogger<SharePicService> logger)
{
    public async Task<byte[]> GenerateSharePicAsync(string templateId, IDictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        Log.LookingForTemplateDirectory(logger, templateId);
        
        var templateDir= Path.Combine(templateBaseDir, templateId);
        var templateFiles = Directory.GetFiles(templateDir, "*", SearchOption.AllDirectories);
        var indexFileName = templateFiles
            .FirstOrDefault(f => Path.GetFileName(f).Equals("index.html", StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException("No index.html file found in template directory", templateDir);
        
        Log.FoundTemplate(logger, templateId, templateDir);
        
        var templateHtml = await File.ReadAllTextAsync(indexFileName, cancellationToken);
        var filledTemplate = FillTemplate(templateHtml, data);
        return await gotenbergClient.ConvertHtmlToPngAsync(filledTemplate, cancellationToken);
    }
    
    private string FillTemplate(string templateHtml, IDictionary<string, object> data)
    {
        var template = Template.Parse(templateHtml);
        return template.Render(data);
    }
}