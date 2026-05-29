using Microsoft.AspNetCore.SignalR;

namespace SmartQ.API.Hubs;

public class QueueHub : Hub
{
    public const string Route = "/hubs/queue";
}
