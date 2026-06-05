using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartQ.Application.DTOs;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokensController : ControllerBase
{
    private readonly ITokenService _tokens;

    public TokensController(ITokenService tokens) => _tokens = tokens;

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateTokenRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await _tokens.GenerateTokenAsync(request, ct));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Tokens_TokenNo") == true)
        {
            return Conflict(new { message = "Token number already exists. Please try again." });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var token = await _tokens.GetTokenAsync(id, ct);
        return token == null ? NotFound() : Ok(token);
    }

    [HttpPost("{tokenId:int}/recall")]
    public async Task<IActionResult> Recall(int tokenId, CancellationToken ct) =>
        ToResult(await _tokens.RecallTokenAsync(tokenId, ct));

    [HttpPost("{tokenId:int}/start")]
    public async Task<IActionResult> Start(int tokenId, CancellationToken ct) =>
        ToResult(await _tokens.StartTokenAsync(tokenId, ct));

    [HttpPost("{tokenId:int}/complete")]
    public async Task<IActionResult> Complete(int tokenId, CancellationToken ct) =>
        ToResult(await _tokens.CompleteTokenAsync(tokenId, ct));

    [HttpPost("{tokenId:int}/skip")]
    public async Task<IActionResult> Skip(int tokenId, CancellationToken ct) =>
        ToResult(await _tokens.SkipTokenAsync(tokenId, ct));

    [HttpPost("{tokenId:int}/transfer")]
    public async Task<IActionResult> Transfer(int tokenId, [FromBody] TransferRequest request, CancellationToken ct) =>
        ToResult(await _tokens.TransferTokenAsync(tokenId, request.TargetCounterId, ct));

    private IActionResult ToResult(object? result) =>
        result == null ? NotFound() : Ok(result);
}

public record TransferRequest(int TargetCounterId);
