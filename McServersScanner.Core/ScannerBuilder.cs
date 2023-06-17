using System.Net;
using System.Threading.Tasks.Dataflow;

namespace McServersScanner.Core;

/// <summary>
/// A builder for <see cref="Scanner"/> class and variables
/// </summary>
public sealed class ScannerBuilder
{
    public const int DEFAULT_CONNECTION_LIMIT = 1000;

    /// <summary>
    /// Block of Ips to scan
    /// </summary>
    public BufferBlock<IPAddress> Ips = null!;

    /// <summary>
    /// Array of Ports to scan
    /// </summary>
    public ushort[] Ports = { 25565 };

    /// <summary>
    /// Amount of active connections app can handle at the same time
    /// </summary>
    public int ConnectionLimit { get; set; } = DEFAULT_CONNECTION_LIMIT;

    /// <summary>
    /// Amount of bytes send/received by network per second
    /// </summary>
    public int BandwidthLimit = 1024 * 1024;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public double Timeout { get; set; } = 10;

    /// <summary>
    /// Supplies <see cref="Ips"/> with <see cref="IPAddress"/>
    /// </summary>
    public Task AddIpAddresses = Task.CompletedTask;

    /// <summary>
    /// Amount of ips to scan
    /// </summary>
    public long IpsCount { get; set; } = 1;

    /// <summary>
    /// Builds the <see cref="Scanner"/>
    /// </summary>
    /// <returns>A configured <see cref="Scanner"/></returns>
    public Scanner Build()
    {
        Scanner builtScanner = new(Ips)
        {
            Ports = Ports,
            ConnectionLimit = ConnectionLimit,
            BandwidthLimit = BandwidthLimit,
            Timeout = Timeout,
            AddIpAddresses = AddIpAddresses,
            TotalIps = IpsCount
                       * Ports.Length //TODO: remove TotalIps and IPsCount and replace it with Enumerable.Count or custom Count method
        };

        return builtScanner;
    }
}