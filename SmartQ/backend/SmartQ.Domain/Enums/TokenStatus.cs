namespace SmartQ.Domain.Enums;

public enum TokenStatus
{
    WAITING = 0,
    CALLED = 1,
    SERVING = 2,
    COMPLETED = 3,
    SKIPPED = 4,
    CANCELLED = 5,
    TRANSFERRED = 6
}
