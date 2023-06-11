using System.Net;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace McServersScanner.Core;

/// <summary>
/// A builder for <see cref="Scanner"/> class and variables
/// </summary>
public sealed class ScannerBuilder
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

    /// <summary>
    /// Builds the <see cref="Scanner"/>
    /// </summary>
    /// <returns>A configured <see cref="Scanner"/></returns>
    public Scanner Build()
    {
        Scanner builtScanner = new(Ips);

        builtScanner.Ports = Ports ?? builtScanner.Ports;
        builtScanner.ConnectionLimit = ConnectionLimit ?? builtScanner.ConnectionLimit;
        builtScanner.BandwidthLimit = BandwidthLimit ?? builtScanner.BandwidthLimit;
        builtScanner.Timeout = Timeout ?? builtScanner.Timeout;
        builtScanner.AddIpAddresses = AddIpAddresses ?? builtScanner.AddIpAddresses;
        builtScanner.TotalIps = TotalIps ?? builtScanner.TotalIps;

        return builtScanner;
    }
}