using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly IConfigurationService _config;

    public SettingsController(IConfigurationService config) => _config = config;

    [HttpGet("public")]
    public async Task<IActionResult> GetPublic(CancellationToken ct) =>
        Ok(await _config.GetPublicSettingsAsync(ct));
}
