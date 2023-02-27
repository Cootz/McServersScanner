using CommunityToolkit.HighPerformance.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace McServersScanner.Network
{
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
        private Socket Client { get; set; }
        
        /// <summary>
        /// Time when connection started
        /// </summary>
        private DateTime InitTime { get; set; }

        /// <summary>
        /// Invokes on successful connection
        /// </summary>
        private readonly AsyncCallback? connectionCallBack;

        public McClient(string ip, ushort port) : this(IPAddress.Parse(ip), port) { }
        public McClient(string ip, ushort port, Action<IAsyncResult> onConnection) : this(IPAddress.Parse(ip), port, onConnection) { }

        public McClient(IPAddress ip, ushort port)
        {
            IpEndPoint = new IPEndPoint(ip, port);
            Client = new(IpEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            InitTime = DateTime.Now;
        }

        public McClient(IPAddress ip, ushort port, Action<IAsyncResult> onConnection) : this(ip, port)
        {
            connectionCallBack = new(onConnection);
        }

        /// <summary>
        /// Begins an asynchronous request for a remote host connection.
        /// </summary>
        public IAsyncResult BeginConnect()
        {
            return Client.BeginConnect(IpEndPoint, connectionCallBack, this);
        }

        /// <summary>
        /// Gets a value that indicates whether a Socket is connected to a remote host
        /// </summary>
        public bool isConnected => Client.Connected;

        /// <summary>
        /// Time when connection started
        /// </summary>
        public DateTime initDateTime => InitTime;

        /// <summary>
        /// Tries to get information from server
        /// </summary>
        /// <remarks>
        /// Client should be connected to server
        /// </remarks>
        public async Task<string> GetServerInfo()
        {
            int protocolVersion = 761;
            StringBuilder response = new StringBuilder();

            try
            {
                //preparing packet
                McPacket<HandshakePacket> packet = new(new HandshakePacket(IpEndPoint.Address, protocolVersion, (ushort)IpEndPoint.Port));

                //Send handshake
                var handshake = Client.SendAsync(packet.ToArray(), SocketFlags.None);

                //Send ping req
                byte[] pingData = { 1, 0 };
                var request = Client.SendAsync(pingData, SocketFlags.None);

                byte[] buffer = new byte[1024];
                int bytesReceived = 0;

                await Task.WhenAll(handshake, request);//Waiting for the packets to be sent

                do
                {
                    bytesReceived = await Client.ReceiveAsync(buffer, SocketFlags.None);
                    response.Append(Encoding.UTF8.GetString(buffer));
                } while (bytesReceived > 0);
            }
            catch
            {
                return string.Empty;
            }

            if (response.Length > 5)
                return StringPool.Shared.GetOrAdd(response.Remove(0, 5).ToString());
            else
                return string.Empty;
        }

        /// <summary>
        /// Asynchronously disconnects from server
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync() => await Client.DisconnectAsync(false);

        public void Dispose()
        {
            Client.Dispose();
            Disposed = true;
        }
    }
}
