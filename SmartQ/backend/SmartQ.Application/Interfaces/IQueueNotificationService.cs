namespace SmartQ.Application.Interfaces;

public interface IQueueNotificationService
{
    Task TokenGeneratedAsync(object payload, CancellationToken ct = default);
    Task TokenCalledAsync(object payload, CancellationToken ct = default);
    Task TokenRecalledAsync(object payload, CancellationToken ct = default);
    Task TokenStartedAsync(object payload, CancellationToken ct = default);
    Task TokenCompletedAsync(object payload, CancellationToken ct = default);
    Task TokenSkippedAsync(object payload, CancellationToken ct = default);
    Task QueueUpdatedAsync(object payload, CancellationToken ct = default);
    Task DisplayUpdatedAsync(object payload, CancellationToken ct = default);
}
