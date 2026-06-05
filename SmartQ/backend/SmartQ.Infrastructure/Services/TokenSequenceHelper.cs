using Microsoft.EntityFrameworkCore;
using SmartQ.Domain.Entities;
using SmartQ.Infrastructure.Persistence;

namespace SmartQ.Infrastructure.Services;

internal static class TokenSequenceHelper
{
    public static async Task<(int SequenceNo, string TokenNo)> NextAsync(
        SmartQDbContext db, int subServiceId, string tokenPrefix, CancellationToken ct)
    {
        var today = DateTime.Today;
        var sequence = await db.DailyTokenSequences
            .FirstOrDefaultAsync(d => d.SequenceDate == today && d.SubServiceId == subServiceId, ct);

        var sequenceDate = DateOnly.FromDateTime(today);
        var maxExisting = await db.Tokens
            .Where(t => t.SubServiceId == subServiceId && t.SequenceDate == sequenceDate)
            .MaxAsync(t => (int?)t.SequenceNo, ct) ?? 0;

        if (sequence == null)
        {
            sequence = new DailyTokenSequence
            {
                SequenceDate = today,
                SubServiceId = subServiceId,
                TokenPrefix = tokenPrefix,
                LastNumber = maxExisting,
                UpdatedAt = DateTime.Now
            };
            db.DailyTokenSequences.Add(sequence);
        }
        else
        {
            sequence.TokenPrefix = tokenPrefix;
            if (sequence.LastNumber < maxExisting)
                sequence.LastNumber = maxExisting;
        }

        sequence.LastNumber++;
        sequence.UpdatedAt = DateTime.Now;

        var sequenceNo = sequence.LastNumber;
        return (sequenceNo, $"{tokenPrefix}-{sequenceNo:D3}");
    }
}
