using System.Runtime.CompilerServices;

namespace McServersScanner.Core.IO;

/// <summary>
/// Handle throttling from multiple <see cref="SharedThrottledStream"/>
/// </summary>
public interface IThrottleManager
{
    /// <summary>
    /// Handle throttling from multiple threads
    /// </summary>
    /// <returns>True if throttle was successful, otherwise false</returns>
    bool Throttle(int bytes);

    TaskAwaiter GetAwaiter();
}