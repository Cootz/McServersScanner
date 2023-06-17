using System.Runtime.CompilerServices;

namespace McServersScanner.Core.IO;

public class ThrottleManager
{
    public int CurrentQuota;

    private readonly System.Timers.Timer timer = new();

    public readonly int MaxBytesPerSecond;

    public ThrottleManager(int maxBytesPerSecond)
    {
        MaxBytesPerSecond = maxBytesPerSecond;

        CurrentQuota = maxBytesPerSecond;

        timer.Interval = 1000;
        timer.AutoReset = true;
        timer.Elapsed += (_, _) => CurrentQuota = MaxBytesPerSecond;

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

    public TaskAwaiter GetAwaiter() =>
        Task.Run(() => SpinWait.SpinUntil(() => CurrentQuota == MaxBytesPerSecond)).GetAwaiter();

    public class ThrottleAwaiter : ICriticalNotifyCompletion, INotifyCompletion
    {
        private readonly ThrottleManager manager;

        public ThrottleAwaiter(ThrottleManager manager) => this.manager = manager;

        public void GetResult()
        {
        }

        public bool IsCompleted
        {
            get => manager.CurrentQuota == manager.MaxBytesPerSecond;
        }

        public void OnCompleted(Action continuation)
        {
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }
    }
}