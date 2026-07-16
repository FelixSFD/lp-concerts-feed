using Microsoft.AspNetCore.Mvc;

namespace Server.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class VenuesController(ILogger<VenuesController> logger) : ControllerBase
{
    
}