using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/staff-console")]
[Authorize(Roles = "STAFF")]
public class StaffConsoleController : ControllerBase
{
    private readonly IStaffConsoleService _staff;
    private readonly IStaffSessionService _sessions;
    private readonly ICurrentUserService _currentUser;

    public StaffConsoleController(
        IStaffConsoleService staff,
        IStaffSessionService sessions,
        ICurrentUserService currentUser)
    {
        _staff = staff;
        _sessions = sessions;
        _currentUser = currentUser;
    }

    [HttpGet("context")]
    public async Task<IActionResult> GetContext(CancellationToken ct) =>
        Ok(await _staff.GetContextAsync(await RequireCounterIdAsync(ct), ct));

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct) =>
        Ok(await _staff.GetSummaryAsync(await RequireCounterIdAsync(ct), ct));

    [HttpGet("active-session")]
    public async Task<IActionResult> GetActiveSession(CancellationToken ct) =>
        Ok(await _staff.GetActiveSessionAsync(await RequireCounterIdAsync(ct), ct));

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue([FromQuery] string scope = "my-services", CancellationToken ct = default) =>
        Ok(await _staff.GetQueueAsync(await RequireCounterIdAsync(ct), scope, ct));

    [HttpPost("call-next")]
    public async Task<IActionResult> CallNext(CancellationToken ct) =>
        Ok(await _staff.CallNextAsync(await RequireCounterIdAsync(ct), _currentUser.UserId, ct));

    [HttpPost("tokens/{tokenId:int}/recall")]
    public async Task<IActionResult> Recall(int tokenId, CancellationToken ct)
        => ToResult(await _staff.RecallAsync(tokenId, await RequireCounterIdAsync(ct), _currentUser.UserId, ct));

    [HttpPost("tokens/{tokenId:int}/start-service")]
    public async Task<IActionResult> StartService(int tokenId, CancellationToken ct)
        => ToResult(await _staff.StartServiceAsync(tokenId, await RequireCounterIdAsync(ct), _currentUser.UserId, ct));

    [HttpPost("tokens/{tokenId:int}/complete")]
    public async Task<IActionResult> Complete(int tokenId, CancellationToken ct)
        => ToResult(await _staff.CompleteAsync(tokenId, await RequireCounterIdAsync(ct), _currentUser.UserId, ct));

    [HttpPost("tokens/{tokenId:int}/no-show")]
    public async Task<IActionResult> NoShow(int tokenId, CancellationToken ct)
        => ToResult(await _staff.NoShowAsync(tokenId, await RequireCounterIdAsync(ct), _currentUser.UserId, ct));

    [HttpPost("tokens/{tokenId:int}/cancel")]
    public async Task<IActionResult> Cancel(int tokenId, CancellationToken ct)
        => ToResult(await _staff.CancelAsync(tokenId, await RequireCounterIdAsync(ct), _currentUser.UserId, ct));

    [HttpPost("tokens/{tokenId:int}/transfer")]
    public async Task<IActionResult> Transfer(int tokenId, [FromBody] StaffTransferTokenRequest request, CancellationToken ct)
        => ToResult(await _staff.TransferAsync(tokenId, request, await RequireCounterIdAsync(ct), _currentUser.UserId, ct));

    [HttpGet("token-history")]
    public async Task<IActionResult> TokenHistory(
        [FromQuery] DateTime? date,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? status,
        [FromQuery] int? serviceId,
        CancellationToken ct)
        => Ok(await _staff.GetTokenHistoryAsync(
            await RequireCounterIdAsync(ct), date, dateFrom, dateTo, status, serviceId, ct));

    [HttpGet("performance")]
    public async Task<IActionResult> Performance([FromQuery] string range = "today", CancellationToken ct = default)
        => Ok(await _staff.GetPerformanceAsync(_currentUser.UserId, await RequireCounterIdAsync(ct), range, ct));

    [HttpGet("notifications")]
    public async Task<IActionResult> Notifications(CancellationToken ct)
        => Ok(await _staff.GetNotificationsAsync(await RequireCounterIdAsync(ct), ct));

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => Ok(await _staff.GetDashboardAsync(await RequireCounterIdAsync(ct), ct));

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
    public async Task<IActionResult> MyCounter(CancellationToken ct)
        => Ok(await _staff.GetMyCounterAsync(await RequireCounterIdAsync(ct), _currentUser.UserId, ct));

    [HttpPost("status")]
    public async Task<IActionResult> UpdateCounterStatus([FromBody] StaffCounterStatusRequest request, CancellationToken ct)
        => Ok(await _staff.UpdateCounterStatusAsync(await RequireCounterIdAsync(ct), request.Status, ct));

    private async Task<int> RequireCounterIdAsync(CancellationToken ct)
    {
        if (_currentUser.UserId is not int userId)
            throw new InvalidOperationException("Unauthorized.");

        var counterId = await _sessions.ResolveCounterIdForStaffAsync(userId, ct);
        if (counterId is null)
            throw new InvalidOperationException("Please select a counter first.");

        return counterId.Value;
    }

    private IActionResult ToResult(object? result) =>
        result == null ? NotFound() : Ok(result);
}
