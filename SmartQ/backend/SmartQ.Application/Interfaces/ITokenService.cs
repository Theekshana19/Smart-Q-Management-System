using SmartQ.Application.DTOs;

namespace SmartQ.Application.Interfaces;

public interface ITokenService
{
    Task<GenerateTokenResponse> GenerateTokenAsync(GenerateTokenRequest request, CancellationToken ct = default);
    Task<TokenDetailDto?> GetTokenAsync(int id, CancellationToken ct = default);
    Task<TokenDetailDto?> RecallTokenAsync(int tokenId, CancellationToken ct = default);
    Task<TokenDetailDto?> StartTokenAsync(int tokenId, CancellationToken ct = default);
    Task<TokenDetailDto?> CompleteTokenAsync(int tokenId, CancellationToken ct = default);
    Task<TokenDetailDto?> SkipTokenAsync(int tokenId, CancellationToken ct = default);
    Task<TokenDetailDto?> TransferTokenAsync(int tokenId, int targetCounterId, CancellationToken ct = default);
}
