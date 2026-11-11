using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Scriban;
using Service.SharePics.Gotenberg;

namespace Service.SharePics;

public partial class SharePicService(IGotenbergClient gotenbergClient, string templateBaseDir, ILogger<SharePicService> logger)
{
    public async Task<byte[]> GenerateSharePicAsync(string templateId, IDictionary<string, object> data, uint? width = null, uint? height = null, CancellationToken cancellationToken = default)
    {
        Log.LookingForTemplateDirectory(logger, templateId);
        
        var templateDir= Path.Combine(templateBaseDir, templateId);
        var templateFiles = Directory.GetFiles(templateDir, "*", SearchOption.AllDirectories);
        var indexFileName = templateFiles
            .FirstOrDefault(f => Path.GetFileName(f).Equals("index.html", StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException("No index.html file found in template directory", templateDir);
        
        var supportingFiles = GetSupportingFiles(templateBaseDir);
        var additionalFiles = templateFiles
            .Where(f => !Path.GetFileName(f).Equals("index.html", StringComparison.OrdinalIgnoreCase))
            .Concat(supportingFiles)
            .Select(f => 
                new GotenbergResourceFile
                {
                    FileName = Path.GetRelativePath(templateDir, f),
                    FileContent = File.ReadAllBytes(f),
                }
            )
            .ToArray();
        
        Log.FoundTemplate(logger, templateId, templateDir);
        
        var templateHtml = await File.ReadAllTextAsync(indexFileName, cancellationToken);
        templateHtml = RewriteAssetPaths(templateHtml);
        var filledTemplate = FillTemplate(templateHtml, data);
        var outputConfig = new GotenbergOutputConfig
        {
            Width = width,
            Height = height,
        };
        return await gotenbergClient.ConvertHtmlToPngAsync(filledTemplate, additionalFiles, outputConfig, cancellationToken);
    }
    
    private string FillTemplate(string templateHtml, IDictionary<string, object> data)
    {
        var template = Template.Parse(templateHtml);
        return template.Render(data);
    }
    
    /// <summary>
    /// Loads the supporting files from the "_base" directory in the template base directory.
    /// </summary>
    /// <param name="templateBaseDir"></param>
    /// <returns></returns>
    private static string[] GetSupportingFiles(string templateBaseDir)
    {
        var supportingFilesDir = Path.Combine(templateBaseDir, "_base");
        return
        [
            .. Directory.GetFiles(supportingFilesDir, "*", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).Equals("index.html", StringComparison.OrdinalIgnoreCase))
        ];
    }
    
    /// <summary>
    /// Gotenberg requires all assets to be in the same directory as the HTML file.
    /// This method rewrites the asset paths in the HTML to only include the file name, so that Gotenberg can find them.
    /// </summary>
    /// <param name="html">input HTML template</param>
    /// <returns>modified HTML</returns>
    private static string RewriteAssetPaths(string html)
    {
        var regex = FileSrcPathRegex();

        return regex.Replace(html, match =>
        {
            var path = match.Groups["path"].Value;

            if (Uri.TryCreate(path, UriKind.Absolute, out _)
                || path.StartsWith('#'))
                return match.Value;

            var fileName = Path.GetFileName(path);

            return $"{match.Groups["attribute"].Value}=\"{fileName}\"";
        });
    }

    [GeneratedRegex("""(?<attribute>src|href)="(?<path>[^"]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex FileSrcPathRegex();
}