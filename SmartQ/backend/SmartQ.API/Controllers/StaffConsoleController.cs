using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/staff-console")]
public class StaffConsoleController : ControllerBase
{
    private readonly IStaffConsoleService _staff;

    public StaffConsoleController(IStaffConsoleService staff) => _staff = staff;

    [HttpGet("context")]
    public async Task<IActionResult> GetContext([FromQuery] int counterId, CancellationToken ct) =>
        Ok(await _staff.GetContextAsync(counterId, ct));

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int counterId, CancellationToken ct) =>
        Ok(await _staff.GetSummaryAsync(counterId, ct));

    [HttpGet("active-session")]
    public async Task<IActionResult> GetActiveSession([FromQuery] int counterId, CancellationToken ct) =>
        Ok(await _staff.GetActiveSessionAsync(counterId, ct));

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue([FromQuery] int counterId, [FromQuery] string scope = "my-services", CancellationToken ct = default) =>
        Ok(await _staff.GetQueueAsync(counterId, scope, ct));

    [HttpPost("{counterId:int}/call-next")]
    public async Task<IActionResult> CallNext(int counterId, CancellationToken ct) =>
        Ok(await _staff.CallNextAsync(counterId, null, ct));

    [HttpPost("tokens/{tokenId:int}/recall")]
    public async Task<IActionResult> Recall(int tokenId, [FromQuery] int counterId, CancellationToken ct)
        => ToResult(await _staff.RecallAsync(tokenId, counterId, null, ct));

    [HttpPost("tokens/{tokenId:int}/start-service")]
    public async Task<IActionResult> StartService(int tokenId, [FromQuery] int counterId, CancellationToken ct)
        => ToResult(await _staff.StartServiceAsync(tokenId, counterId, null, ct));

    [HttpPost("tokens/{tokenId:int}/complete")]
    public async Task<IActionResult> Complete(int tokenId, [FromQuery] int counterId, CancellationToken ct)
        => ToResult(await _staff.CompleteAsync(tokenId, counterId, null, ct));

    [HttpPost("tokens/{tokenId:int}/no-show")]
    public async Task<IActionResult> NoShow(int tokenId, [FromQuery] int counterId, CancellationToken ct)
        => ToResult(await _staff.NoShowAsync(tokenId, counterId, null, ct));

    [HttpPost("tokens/{tokenId:int}/transfer")]
    public async Task<IActionResult> Transfer(int tokenId, [FromQuery] int counterId, [FromBody] StaffTransferTokenRequest request, CancellationToken ct)
        => ToResult(await _staff.TransferAsync(tokenId, request, counterId, null, ct));

    [HttpGet("token-history")]
    public async Task<IActionResult> TokenHistory([FromQuery] int counterId, [FromQuery] DateTime? date, [FromQuery] string? status, [FromQuery] int? serviceId, CancellationToken ct)
        => Ok(await _staff.GetTokenHistoryAsync(counterId, date, status, serviceId, ct));

    [HttpGet("performance")]
    public async Task<IActionResult> Performance([FromQuery] int? staffUserId, [FromQuery] int counterId, [FromQuery] string range = "today", CancellationToken ct = default)
        => Ok(await _staff.GetPerformanceAsync(staffUserId, counterId, range, ct));

    [HttpGet("notifications")]
    public async Task<IActionResult> Notifications([FromQuery] int counterId, CancellationToken ct)
        => Ok(await _staff.GetNotificationsAsync(counterId, ct));

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] int counterId, CancellationToken ct)
        => Ok(await _staff.GetDashboardAsync(counterId, ct));

    [HttpGet("token-details/{tokenId:int}")]
    public async Task<IActionResult> TokenDetails(int tokenId, CancellationToken ct)
    {
        var result = await _staff.GetTokenDetailsAsync(tokenId, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("transfer-options")]
    public async Task<IActionResult> TransferOptions(CancellationToken ct)
        => Ok(await _staff.GetTransferOptionsAsync(ct));

    [HttpGet("my-counter")]
    public async Task<IActionResult> MyCounter([FromQuery] int counterId, [FromQuery] int? staffUserId, CancellationToken ct)
        => Ok(await _staff.GetMyCounterAsync(counterId, staffUserId, ct));

    [HttpPost("{counterId:int}/status")]
    public async Task<IActionResult> UpdateCounterStatus(int counterId, [FromBody] StaffCounterStatusRequest request, CancellationToken ct)
        => Ok(await _staff.UpdateCounterStatusAsync(counterId, request.Status, ct));

    private IActionResult ToResult(object? result) =>
        result == null ? NotFound() : Ok(result);
}
