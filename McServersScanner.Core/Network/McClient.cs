using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using McServersScanner.Core.IO;
using McServersScanner.Core.Network.Packets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McServersScanner.Core.Network;

public class McClient : IDisposable
{
    /// <summary>
    /// Server IP address + port
    /// </summary>
    public IPEndPoint IpEndPoint { get; init; }

    /// <summary>
    /// Checks if instance disposed
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Client logic
    /// </summary>
    private TcpClient client { get; }

    /// <summary>
    /// Time when connection started
    /// </summary>
    private DateTime initTime { get; set; }

    /// <summary>
    /// Invokes on successful connection
    /// </summary>
    public AsyncCallback? ConnectionCallBack { get; init; }

    /// <summary>
    /// Max number of bytes sent/received by network per second
    /// </summary>
    public int BandwidthLimit { get; init; }

    private readonly IThrottleManager manager;
    private readonly ILogger<McClient>? logger;

    public McClient(string ip, ushort port, IServiceProvider services) : this(IPAddress.Parse(ip), port, services)
    {
    }

    public McClient(IPAddress ip, ushort port, IServiceProvider services)
    {
        IpEndPoint = new IPEndPoint(ip, port);
        client = new TcpClient(IpEndPoint.AddressFamily);

        manager = services.GetService<IThrottleManager>()!;
        logger = services.GetService<ILogger<McClient>>();
    }

    /// <summary>
    /// Begins an asynchronous request for a remote host connection.
    /// </summary>
    public IAsyncResult BeginConnect()
    {
        initTime = DateTime.Now;

        return client.BeginConnect(IpEndPoint.Address, IpEndPoint.Port, ConnectionCallBack, this);
    }

    /// <summary>
    /// Gets a value that indicates whether a Socket is connected to a remote host
    /// </summary>
    public bool IsConnected
    {
        get => client.Connected;
    }

    /// <summary>
    /// Time when connection started
    /// </summary>
    public DateTime InitDateTime
    {
        get => initTime;
    }

    /// <summary>
    /// Tries to get information from server
    /// </summary>
    /// <remarks>
    /// Client should be connected to server
    /// </remarks>
    public async Task<string> GetServerInfo()
    {
        const int protocolVersion = 761;
        string response;

        try
        {
            //preparing packet
            HandshakePacket packet = new(IpEndPoint.Address, protocolVersion, (ushort)IpEndPoint.Port);

            SharedThrottledStream stream = new(client.GetStream(), manager);

            //Send handshake
            await stream.WriteAsync(packet.ToArray());

            //Send ping req
            byte[] pingData = { 1, 0 };
            await stream.WriteAsync(pingData);

            int length = McProtocol.ReadVarInt(stream);

            byte[] buffer = new byte[length];

            _ = await stream.ReadAsync(buffer);

            MemoryStream memoryStream = new(buffer);

            int packetId = memoryStream.ReadByte();

            Debug.Assert(packetId == 0);

            length = McProtocol.ReadVarInt(memoryStream);

            byte[] jsonData = new byte[length];

            _ = await memoryStream.ReadAsync(jsonData);

            response = StringPool.Shared.GetOrAdd(Encoding.UTF8.GetString(jsonData));
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"{nameof(GetServerInfo)} for {IpEndPoint} aborted");
            return string.Empty;
        }

        return StringPool.Shared.GetOrAdd(response);
    }

    /// <summary>
    /// Asynchronously disconnects from server
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectAsync() => await client.Client.DisconnectAsync(true);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        client.Dispose();
        Disposed = true;
    }
}