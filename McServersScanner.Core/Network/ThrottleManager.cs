using System.Reactive.Concurrency;

namespace McServersScanner.Core.Network;

public class ThrottleManager
{
    public static ThrottleManager Current { get; private set; } = null!;

    public readonly IScheduler Scheduler;

    private readonly int maxBytesPerSecond;
    private readonly IStopwatch stopwatch;

    private long processed;

    public ThrottleManager(int maxBytesPerSecond, IScheduler scheduler)
    {
        this.maxBytesPerSecond = maxBytesPerSecond;
        Scheduler = scheduler;
        stopwatch = scheduler.StartStopwatch();

        processed = 0;

        Current = this;
    }

    public ThrottleManager(int maxBytesPerSecond) : this(maxBytesPerSecond,
        System.Reactive.Concurrency.Scheduler.Immediate)
    {
    }

    public TimeSpan GetSleepTime(int bytes)
    {
        processed += bytes;

        TimeSpan targetTime = TimeSpan.FromSeconds((double)processed / maxBytesPerSecond);
        TimeSpan actualTime = stopwatch.Elapsed;

        return targetTime - actualTime;
    }
}