using System.Reactive.Concurrency;

namespace McServersScanner.Core.Network
{
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
            this.Scheduler = scheduler;
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

    internal static class WaitHandleHelper
    {
        public static Task WaitOneAsync(this WaitHandle waitHandle)
        {
            if (waitHandle == null)
                throw new ArgumentNullException("waitHandle");

            TaskCompletionSource<bool> tcs = new();

            RegisteredWaitHandle rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle,
                delegate { tcs.TrySetResult(true); }, null, -1, true);

            Task<bool> t = tcs.Task;
            t.ContinueWith(_ => rwh.Unregister(null));

            return t;
        }
    }
}