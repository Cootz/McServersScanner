using CommunityToolkit.HighPerformance.Buffers;
using static System.String;

namespace McServersScanner.Core.Utils;

/// <summary>
/// Helps with IO stuff
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IOHelper
{
    /// <summary>
    /// Reads file line by line
    /// </summary>
    /// <param name="path">Path to file</param>
    /// <returns><see cref="IEnumerable{string}"/></returns>
    /// <remarks>
    /// Ignores empty lines
    /// </remarks>
    public static IEnumerable<string> ReadLineByLine(string path)
    {
        using StreamReader reader = new(path);

        while (reader.ReadLine() is { } line)
            if (!IsNullOrEmpty(line))
                yield return StringPool.Shared.GetOrAdd(line);
    }

    /// <summary>
    /// Simulates <see cref="ReadLineByLine(string)"/> behavior to calculate it length
    /// </summary>
    /// <param name="path">Path to file</param>
    /// <returns>Amount of non empty lines</returns>
    /// <remarks>
    /// Ignores empty lines
    /// </remarks>
    public static long GetLinesCount(string path)
    {
        long count = 0;

        using StreamReader reader = new(path);

        while (reader.ReadLine() is { } line)
            if (!IsNullOrEmpty(line))
                count++;

        return count;
    }
}