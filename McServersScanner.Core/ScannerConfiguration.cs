using System.Net;
using System.Threading.Tasks.Dataflow;

namespace McServersScanner.Core;

/// <summary>
/// Provide variables for using various scanner options
/// </summary>
public class ScannerConfiguration
{
    /// <summary>
    /// Block of Ips to scan
    /// </summary>
    public BufferBlock<IPAddress> Ips = null!;

    /// <summary>
    /// Array of Ports to scan
    /// </summary>
    public ushort[]? Ports = null;

    /// <summary>
    /// Amount of active connections app can handle at the same time
    /// </summary>
    public int? ConnectionLimit = null;

    /// <summary>
    /// Amount of bytes send/received by network per second
    /// </summary>
    public int? BandwidthLimit = null;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public double? Timeout = null;

    /// <summary>
    /// Supplies <see cref="Ips"/> with <see cref="IPAddress"/>
    /// </summary>
    public Task? AddIpAddresses = null;

    /// <summary>
    /// Amount of ips to scan
    /// </summary>
    public long? TotalIps = null;
}