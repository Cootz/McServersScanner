using System.Net;
using System.Net.Sockets;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

namespace McServersScanner.Core.Network;

public class McClient : IDisposable
{
    /// <summary>
    /// Server IP address + port
    /// </summary>
    public IPEndPoint IpEndPoint { get; private set; }

    /// <summary>
    /// Checks if instance disposed
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Client logic
    /// </summary>
    private Socket client { get; set; }

    /// <summary>
    /// Time when connection started
    /// </summary>
    private DateTime initTime { get; set; }

    /// <summary>
    /// Invokes on successful connection
    /// </summary>
    private readonly AsyncCallback? connectionCallBack;

    /// <summary>
    /// Max number of bytes sent/received by network per second
    /// </summary>
    public int BandwidthLimit { get; set; }

    public McClient(string ip, ushort port, int bandwidthLimit) : this(IPAddress.Parse(ip), port, bandwidthLimit)
    {
    }

    public McClient(string ip, ushort port, Action<IAsyncResult> onConnection, int bandwidthLimit) : this(
        IPAddress.Parse(ip), port, onConnection, bandwidthLimit)
    {
    }

    public McClient(IPAddress ip, ushort port, int bandwidthLimit)
    {
        IpEndPoint = new IPEndPoint(ip, port);
        client = new Socket(IpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        BandwidthLimit = bandwidthLimit;

        initTime = DateTime.Now;
    }

    public McClient(IPAddress ip, ushort port, Action<IAsyncResult> onConnection, int bandwidthLimit) :
        this(ip, port, bandwidthLimit) => connectionCallBack = new AsyncCallback(onConnection);

    /// <summary>
    /// Begins an asynchronous request for a remote host connection.
    /// </summary>
    public IAsyncResult BeginConnect() => client.BeginConnect(IpEndPoint, connectionCallBack, this);

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
        int protocolVersion = 761;
        StringBuilder response = new();

        try
        {
            //preparing packet
            McPacket<HandshakePacket> packet =
                new(new HandshakePacket(IpEndPoint.Address, protocolVersion, (ushort)IpEndPoint.Port));

            SharedThrottledStream stream = new(new NetworkStream(client, true));

            //Send handshake
            Task? handshake = stream.WriteAsync(packet.ToArray()).AsTask();

            //Send ping req
            byte[] pingData = { 1, 0 };
            Task? request = stream.WriteAsync(pingData).AsTask();

            byte[] buffer = new byte[1024];
            int bytesReceived;

            await Task.WhenAll(handshake, request); //Waiting for the packets to be sent

            do
            {
                bytesReceived = await stream.ReadAsync(buffer);
                response.Append(StringPool.Shared.GetOrAdd(Encoding.UTF8.GetString(buffer)));
            } while (bytesReceived > 0);
        }
        catch
        {
            return string.Empty;
        }

        return response.Length > 5 ? StringPool.Shared.GetOrAdd(response.Remove(0, 5).ToString()) : string.Empty;
    }

    /// <summary>
    /// Asynchronously disconnects from server
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectAsync() => await client.DisconnectAsync(false);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        client.Dispose();
        Disposed = true;
    }
}