using System.Runtime.CompilerServices;

namespace McServersScanner.Core.IO;

public class ThrottleManager
{
    public volatile int CurrentQuota;

    private readonly System.Timers.Timer timer = new();

    private readonly int maxBytesPerSecond;

    public ThrottleManager(int maxBytesPerSecond)
    {
        this.maxBytesPerSecond = maxBytesPerSecond;

        CurrentQuota = maxBytesPerSecond;

        timer.Interval = 1000;
        timer.AutoReset = true;
        timer.Elapsed += (_, _) => CurrentQuota = maxBytesPerSecond;
    }

    public ThrottleAwaiter GetAwaiter() => new(this);

    public class ThrottleAwaiter : INotifyCompletion
    {
        private readonly ThrottleManager manager;

        public ThrottleAwaiter(ThrottleManager manager) => this.manager = manager;

        public void GetResult()
        {
        }

        public bool IsCompleted
        {
            get => manager.CurrentQuota == manager.maxBytesPerSecond;
        }

        public void OnCompleted(Action continuation)
        {
        }
    }
}