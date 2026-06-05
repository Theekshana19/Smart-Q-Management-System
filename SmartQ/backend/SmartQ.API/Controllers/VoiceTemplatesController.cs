using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/voice-templates")]
public class VoiceTemplatesController : ControllerBase
{
    private readonly IConfigurationService _config;

    public VoiceTemplatesController(IConfigurationService config) => _config = config;

    [HttpGet("template")]
    public async Task<IActionResult> GetTemplate(
        [FromQuery] string languageCode = "EN",
        [FromQuery] string eventType = "TOKEN_CALLED",
        CancellationToken ct = default)
    {
        var tpl = await _config.GetRuntimeVoiceTemplateAsync(languageCode, eventType, ct);
        return tpl == null ? NotFound() : Ok(tpl);
    }
}
