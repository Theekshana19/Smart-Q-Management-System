using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountersController : ControllerBase
{
    private readonly ICounterService _counters;

    public CountersController(ICounterService counters) => _counters = counters;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await _counters.GetCountersAsync(ct));

    [HttpGet("{counterId:int}/queue")]
    public async Task<IActionResult> GetQueue(int counterId, CancellationToken ct) =>
        Ok(await _counters.GetCounterQueueAsync(counterId, ct));

    [HttpPost("{counterId:int}/call-next")]
    public async Task<IActionResult> CallNext(int counterId, CancellationToken ct)
    {
        var result = await _counters.CallNextAsync(counterId, ct: ct);
        return result == null ? NotFound(new { message = "No waiting tokens." }) : Ok(result);
    }

    [HttpGet("{counterId:int}/console-summary")]
    public async Task<IActionResult> GetConsoleSummary(int counterId, CancellationToken ct) =>
        Ok(await _counters.GetStaffConsoleSummaryAsync(counterId, ct));
}
