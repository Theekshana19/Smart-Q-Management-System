using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin) => _admin = admin;

    [HttpGet("dashboard/summary")]
    public async Task<IActionResult> DashboardSummary(CancellationToken ct) =>
        Ok(await _admin.GetDashboardSummaryAsync(ct));

    [HttpGet("services/summary")]
    public async Task<IActionResult> ServiceSummary(CancellationToken ct) =>
        Ok(await _admin.GetServiceManagementSummaryAsync(ct));

    [HttpGet("services")]
    public async Task<IActionResult> GetServices(CancellationToken ct) =>
        Ok(await _admin.GetServicesAsync(ct));

    [HttpPost("services")]
    public async Task<IActionResult> CreateService([FromBody] UpsertServiceRequest request, CancellationToken ct) =>
        Ok(await _admin.CreateServiceAsync(request, ct));

    [HttpPut("services/{id:int}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] UpsertServiceRequest request, CancellationToken ct)
    {
        var result = await _admin.UpdateServiceAsync(id, request, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("sub-services")]
    public async Task<IActionResult> GetSubServices(CancellationToken ct) =>
        Ok(await _admin.GetSubServicesAsync(ct));

    [HttpPost("sub-services")]
    public async Task<IActionResult> CreateSubService([FromBody] UpsertSubServiceRequest request, CancellationToken ct) =>
        Ok(await _admin.CreateSubServiceAsync(request, ct));

    [HttpPut("sub-services/{id:int}")]
    public async Task<IActionResult> UpdateSubService(int id, [FromBody] UpsertSubServiceRequest request, CancellationToken ct)
    {
        var result = await _admin.UpdateSubServiceAsync(id, request, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("counters")]
    public async Task<IActionResult> GetCounters(CancellationToken ct) =>
        Ok(await _admin.GetCountersAsync(ct));

    [HttpPost("counters")]
    public async Task<IActionResult> CreateCounter([FromBody] UpsertCounterRequest request, CancellationToken ct) =>
        Ok(await _admin.CreateCounterAsync(request, ct));

    [HttpPut("counters/{id:int}")]
    public async Task<IActionResult> UpdateCounter(int id, [FromBody] UpsertCounterRequest request, CancellationToken ct)
    {
        var result = await _admin.UpdateCounterAsync(id, request, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct) =>
        Ok(await _admin.GetSettingsAsync(ct));

    [HttpPut("settings/{id:int}")]
    public async Task<IActionResult> UpdateSetting(int id, [FromBody] UpdateSettingRequest request, CancellationToken ct)
    {
        var result = await _admin.UpdateSettingAsync(id, request, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("reports/token-history")]
    public async Task<IActionResult> TokenHistory([FromQuery] TokenHistoryFilter filter, CancellationToken ct) =>
        Ok(await _admin.GetTokenHistoryReportAsync(filter, ct));
}
