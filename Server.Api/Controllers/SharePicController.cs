using Microsoft.AspNetCore.Mvc;
using Service.SharePics;

namespace Server.Api.Controllers;

[ApiController]
[Route("v3/sharePics")]
public class SharePicController(SharePicService sharePicService) : ControllerBase
{
    [HttpGet("{templateId}")]
    public async Task<FileResult> GenerateSharePicAsync([FromRoute] string templateId, CancellationToken cancellationToken)
    {
        var templateDir = Path.Combine(AppContext.BaseDirectory, "Templates", templateId);
        var imageBytes = await sharePicService.GenerateSharePicAsync(templateDir, new Dictionary<string, object>(), cancellationToken);
        return File(imageBytes, "image/png");
    }
}