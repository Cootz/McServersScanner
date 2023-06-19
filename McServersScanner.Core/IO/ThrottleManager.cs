using System.Runtime.CompilerServices;

namespace McServersScanner.Core.IO;

public class ThrottleManager
{
    public int CurrentQuota;

    private readonly System.Timers.Timer timer = new();

    public readonly int MaxBytesPerSecond;

    private TaskCompletionSource tcs = new();

    public ThrottleManager(int maxBytesPerSecond)
    {
        MaxBytesPerSecond = maxBytesPerSecond;

        CurrentQuota = maxBytesPerSecond;

        timer.Interval = 1000;
        timer.AutoReset = true;
        timer.Elapsed += (_, _) =>
        {
            CurrentQuota = MaxBytesPerSecond;
            tcs.SetResult();
            tcs = new TaskCompletionSource();
        };

        timer.Start();
    }

    /// <summary>
    /// Handle throttling from multiple threads
    /// </summary>
    /// <returns>True if throttle was successful, otherwise false</returns>
    public bool Throttle(int bytes)
    {
        lock (this)
        {
            if (CurrentQuota < bytes)
                return false;

            CurrentQuota -= bytes;
        }

        return true;
    }

    public TaskAwaiter GetAwaiter() => tcs.Task.GetAwaiter();
}