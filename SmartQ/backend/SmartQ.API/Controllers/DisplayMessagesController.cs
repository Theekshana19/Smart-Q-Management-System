using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/display-messages")]
public class DisplayMessagesController : ControllerBase
{
    private readonly IConfigurationService _config;

    public DisplayMessagesController(IConfigurationService config) => _config = config;

    [HttpGet("public")]
    public async Task<IActionResult> GetPublic([FromQuery] string languageCode = "EN", CancellationToken ct = default) =>
        Ok(await _config.GetPublicDisplayMessagesAsync(languageCode, ct));
}
