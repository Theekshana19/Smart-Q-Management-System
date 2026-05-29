using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DisplayController : ControllerBase
{
    private readonly IDisplayService _display;

    public DisplayController(IDisplayService display) => _display = display;

    [HttpGet("now-serving")]
    public async Task<IActionResult> NowServing(CancellationToken ct) =>
        Ok(await _display.GetNowServingAsync(ct));

    [HttpGet("recently-called")]
    public async Task<IActionResult> RecentlyCalled(CancellationToken ct) =>
        Ok(await _display.GetRecentlyCalledAsync(ct));

    [HttpGet("waiting-queue")]
    public async Task<IActionResult> WaitingQueue(CancellationToken ct) =>
        Ok(await _display.GetWaitingQueueAsync(ct));

    [HttpGet("board")]
    public async Task<IActionResult> Board(CancellationToken ct) =>
        Ok(await _display.GetDisplayBoardAsync(ct));

    [HttpGet("voice-template")]
    public async Task<IActionResult> VoiceTemplate([FromQuery] string eventType = "TOKEN_CALLED", [FromQuery] string languageCode = "EN", CancellationToken ct = default)
    {
        var t = await _display.GetVoiceTemplateAsync(eventType, languageCode, ct);
        return t == null ? NotFound() : Ok(t);
    }
}
