using Microsoft.AspNetCore.SignalR;
using SmartQ.API.Hubs;
using SmartQ.Application.Interfaces;

namespace SmartQ.API.Services;

public class QueueNotificationService : IQueueNotificationService
{
    private readonly IHubContext<QueueHub> _hub;

    public QueueNotificationService(IHubContext<QueueHub> hub) => _hub = hub;

    public Task TokenGeneratedAsync(object payload, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync("TokenGenerated", payload, ct);

    public Task TokenCalledAsync(object payload, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync("TokenCalled", payload, ct);

    public Task TokenRecalledAsync(object payload, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync("TokenRecalled", payload, ct);

    public Task TokenStartedAsync(object payload, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync("TokenStarted", payload, ct);

    public Task TokenCompletedAsync(object payload, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync("TokenCompleted", payload, ct);

    public Task TokenSkippedAsync(object payload, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync("TokenSkipped", payload, ct);

    public Task QueueUpdatedAsync(object payload, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync("QueueUpdated", payload, ct);

    public Task DisplayUpdatedAsync(object payload, CancellationToken ct = default) =>
        _hub.Clients.All.SendAsync("DisplayUpdated", payload, ct);
}
