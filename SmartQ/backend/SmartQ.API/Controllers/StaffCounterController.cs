using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/staff/counters")]
[Authorize(Roles = "STAFF")]
public class StaffCounterController : ControllerBase
{
    private readonly IStaffSessionService _sessions;
    private readonly ICurrentUserService _currentUser;

    public StaffCounterController(IStaffSessionService sessions, ICurrentUserService currentUser)
    {
        _sessions = sessions;
        _currentUser = currentUser;
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable(CancellationToken ct) =>
        Ok(await _sessions.GetAvailableCountersAsync(ct));

    [HttpPost("select")]
    public async Task<IActionResult> Select([FromBody] SelectCounterRequest request, CancellationToken ct)
    {
        if (_currentUser.UserId is not int userId)
            return Unauthorized();

        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            return Ok(await _sessions.SelectCounterAsync(userId, request, ip, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("end-session")]
    public async Task<IActionResult> EndSession(CancellationToken ct)
    {
        if (_currentUser.UserId is not int userId)
            return Unauthorized();

        await _sessions.EndSessionAsync(userId, ct);
        return Ok(new { success = true });
    }

    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateSessionStatusRequest request, CancellationToken ct)
    {
        if (_currentUser.UserId is not int userId)
            return Unauthorized();

        try
        {
            return Ok(await _sessions.UpdateSessionStatusAsync(userId, request.Status, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
