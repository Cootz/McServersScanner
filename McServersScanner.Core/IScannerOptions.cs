using System.Net;
using System.Threading.Tasks.Dataflow;

namespace McServersScanner.Core;

public interface IScannerOptions
{
    /// <summary>
    /// Block of Ips to scan
    /// </summary>
    BufferBlock<IPAddress> Ips { get; }

    /// <summary>
    /// Array of Ports to scan
    /// </summary>
    ushort[] Ports { get; }

    /// <summary>
    /// Amount of active connections app can handle at the same time
    /// </summary>
    int ConnectionLimit { get; }

    /// <summary>
    /// Amount of bytes send/received by network per second
    /// </summary>
    int BandwidthLimit { get; }

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    double Timeout { get; }

    /// <summary>
    /// Supplies <see cref="Ips"/> with <see cref="IPAddress"/>
    /// </summary>
    Task AddIpAddresses { get; }

    /// <summary>
    /// Amount of ips to scan
    /// </summary>
    long TotalIps { get; }
}