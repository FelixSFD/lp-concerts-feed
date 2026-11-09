using Microsoft.AspNetCore.Mvc;
using Service.SharePics;

namespace Server.Api.Controllers;

[ApiController]
[Route("v3/sharePics")]
public class SharePicController(SharePicService sharePicService) : ControllerBase
{
    [HttpPost("{templateId}")]
    public async Task<FileResult> GenerateSharePicAsync([FromRoute] string templateId, [FromQuery] uint? width, [FromQuery] uint? height, [FromBody] Dictionary<string, object> data, CancellationToken cancellationToken)
    {
        var imageBytes = await sharePicService.GenerateSharePicAsync(templateId, data, width, height, cancellationToken);
        return File(imageBytes, "image/png");
    }
}