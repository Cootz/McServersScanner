using System.Runtime.CompilerServices;

namespace McServersScanner.Core.IO;

/// <summary>
/// Handle throttling from multiple <see cref="SharedThrottledStream"/>
/// </summary>
public class ThrottleManager : IThrottleManager
{
    public int CurrentQuota;

    private readonly System.Timers.Timer timer = new();

    public int MaxBytesPerSecond { get; }

    private TaskCompletionSource tcs = new();

    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    public ThrottleManager(int maxBytesPerSecond)
    {
        MaxBytesPerSecond = maxBytesPerSecond;

        CurrentQuota = maxBytesPerSecond;

        timer.Interval = 1000;
        timer.AutoReset = true;
        timer.Elapsed += (_, _) =>
        {
            CurrentQuota = MaxBytesPerSecond;
            while (!tcs.TrySetResult())
            {
            }

            tcs = new TaskCompletionSource();
        };

        timer.Start();
    }

    /// <summary>
    /// Handle throttling from multiple threads
    /// </summary>
    public async Task Throttle(int bytes)
    {
        await semaphoreSlim.WaitAsync();

        while (CurrentQuota < bytes)
        {
            await tcs.Task;
        }

        CurrentQuota -= bytes;

        semaphoreSlim.Release();
    }

    public TaskAwaiter GetAwaiter() => tcs.Task.GetAwaiter();
}