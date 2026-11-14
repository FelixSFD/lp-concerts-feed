using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Prometheus;
using Scriban;
using Service.SharePics.DataStructure;
using Service.SharePics.DataStructure.Templates;
using Service.SharePics.Gotenberg;

namespace Service.SharePics;

/// <summary>
/// Service to generate shareable pictures with stats
/// </summary>
/// <param name="gotenbergClient">Client to connect to Gotenberg, which generates the images from HTML</param>
/// <param name="templateBaseDir">Directory where the template files are located</param>
/// <param name="logger"></param>
public partial class SharePicService(IGotenbergClient gotenbergClient, string templateBaseDir, ILogger<SharePicService> logger)
{
    private static readonly Counter PicsGeneratedCounter = Metrics.CreateCounter(
        "lpshows_share_pic_generated_count", 
        "Counts the number of share-pics that were generated. Labels userId and username are only set if the template is showing user-data",
        new CounterConfiguration
        {
            LabelNames = ["templateId", "userId", "username"]
        });
    
    private static readonly Counter PicsGeneratedBytesCounter = Metrics.CreateCounter(
        "lpshows_share_pic_generated_bytes", 
        "Counts the bytes of share-pics that were generated. Labels userId and username are only set if the template is showing user-data",
        new CounterConfiguration
        {
            LabelNames = ["templateId", "userId", "username"]
        });
    
    private static readonly Histogram PicGenerateDuration = Metrics.CreateHistogram(
        "lpshows_share_pic_generate_duration", 
        "Duration of the generation of share-pics in milliseconds",
        new HistogramConfiguration
        {
            LabelNames = ["templateId", "userId", "username"],
            Buckets = [.. Histogram.PowersOfTenDividedBuckets(1, 4, 20).Skip(15).Take(30)]
        });
    
    /// <summary>
    /// Turns a template into a picture
    /// </summary>
    /// <param name="template">Template to use, including the parameter values</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>Bytes of the generated file</returns>
    public async Task<byte[]> GenerateSharePicAsync(SharePicTemplate template,
        uint? width = null, uint? height = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var pic = await GenerateSharePicAsync(template.TemplateId, template.GetParameterValues(), width, height, template.MainElementSelector,
            cancellationToken);
        
        stopwatch.Stop();
        RegisterGeneratedTemplateMetrics(template, pic.Length, stopwatch.Elapsed);
        
        return pic;
    }
    
    public async Task<byte[]> GenerateSharePicAsync(string templateId, IDictionary<string, object> data, uint? width = null, uint? height = null, string? elementSelector = null, CancellationToken cancellationToken = default)
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

        if (!string.IsNullOrWhiteSpace(elementSelector))
        {
            outputConfig.ElementSelector = elementSelector;
        }
        
        return await gotenbergClient.ConvertHtmlToPngAsync(filledTemplate, additionalFiles, outputConfig, cancellationToken);
    }
    
    private static string FillTemplate(string templateHtml, IDictionary<string, object> data)
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

    /// <summary>
    /// Pushes the values of a generated image to the metrics
    /// </summary>
    /// <param name="template"></param>
    /// <param name="fileSize"></param>
    /// <param name="duration">How long the call took</param>
    private static void RegisterGeneratedTemplateMetrics(SharePicTemplate template, int fileSize, TimeSpan duration)
    {
        string[] labelValues;
        if (template is UserTemplateBase userTemplate)
        {
            labelValues = [userTemplate.TemplateId, userTemplate.UserId, userTemplate.Username];
        }
        else
        {
            labelValues = [template.TemplateId, "", ""];
        }
        
        PicsGeneratedCounter.WithLabels(labelValues).Inc();
        PicsGeneratedBytesCounter.WithLabels(labelValues).Inc(fileSize);
        PicGenerateDuration.WithLabels(labelValues).Observe(duration.TotalMilliseconds);
    }

    [GeneratedRegex("""(?<attribute>src|href)="(?<path>[^"]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex FileSrcPathRegex();
}