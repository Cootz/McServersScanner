using System.Runtime.CompilerServices;

namespace McServersScanner.Core.IO;

/// <summary>
/// Handle throttling from multiple <see cref="SharedThrottledStream"/>
/// </summary>
public interface IThrottleManager
{
    int MaxBytesPerSecond { get; }

    /// <summary>
    /// Handle throttling from multiple threads
    /// </summary>
    Task Throttle(int bytes);

    TaskAwaiter GetAwaiter();
}