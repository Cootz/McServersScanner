using System.Net;
using System.Threading.Tasks.Dataflow;
using McServersScanner.Core.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace McServersScanner.Core;

/// <summary>
/// A builder for <see cref="Scanner"/> class and variables
/// </summary>
public sealed class ScannerBuilder : IScannerOptions
{
    public const int DEFAULT_CONNECTION_LIMIT = 1000;
    public const int DEFAULT_BANDWIDTH_LIMIT = 1024 * 1024;

    /// <summary>
    /// Block of Ips to scan
    /// </summary>
    public BufferBlock<IPAddress> Ips { get; set; } = null!;

    /// <summary>
    /// Array of Ports to scan
    /// </summary>
    public ushort[] Ports { get; set; } = { 25565 };

    /// <summary>
    /// Amount of active connections app can handle at the same time
    /// </summary>
    public int ConnectionLimit { get; set; } = DEFAULT_CONNECTION_LIMIT;

    /// <summary>
    /// Amount of bytes send/received by network per second
    /// </summary>
    public int BandwidthLimit { get; set; } = DEFAULT_BANDWIDTH_LIMIT;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public double Timeout { get; set; } = 10;

    /// <summary>
    /// Supplies <see cref="Ips"/> with <see cref="IPAddress"/>
    /// </summary>
    public Task AddIpAddresses { get; set; } = Task.CompletedTask;

    private readonly IHostBuilder hostBuilder = Host.CreateDefaultBuilder();

    public long TotalIps
    {
        get => IpsCount
               * Ports.Length; //TODO: remove TotalIps and IPsCount and replace it with Enumerable.Count or custom Count method
    }

    /// <summary>
    /// Amount of ips to scan
    /// </summary>
    public long IpsCount { get; set; } = 1;

    /// <summary>
    /// Configures default console logger
    /// </summary>
    public ScannerBuilder ConfigureConsoleLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        return this;
    }

    /// <summary>
    /// Configures default file logger
    /// </summary>
    /// <param name="path">Path to the logfile</param>
    public ScannerBuilder ConfigureFileLogger(string path = "log.txt")
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(path)
            .CreateLogger();

        return this;
    }

    /// <summary>
    /// Builds the <see cref="Scanner"/>
    /// </summary>
    /// <returns>A configured <see cref="Scanner"/></returns>
    public Scanner Build()
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<IThrottleManager, ThrottleManager>(_ => new ThrottleManager(ConnectionLimit));
        });

        hostBuilder.UseSerilog(dispose: true);

        IHost host = hostBuilder.Build();

        Scanner builtScanner = new(Ips, host.Services)
        {
            Ports = Ports,
            ConnectionLimit = ConnectionLimit,
            BandwidthLimit = BandwidthLimit,
            Timeout = Timeout,
            AddIpAddresses = AddIpAddresses,
            TotalIps = TotalIps
        };

        return builtScanner;
    }
}