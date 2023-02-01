using System.Net;
using System.Net.Sockets;
using System.Text;

namespace McServersScanner.Network
{
    public class McClient : IDisposable
    {
        public IPEndPoint IpEndPoint { get; private set; }
        public bool Disposed { get; private set; }
        
        private Socket Client { get; set; }
        private DateTime InitTime { get; set; }
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
                    
        public IAsyncResult BeginConnect()
        {
            return Client.BeginConnect(IpEndPoint, connectionCallBack, this);
        }

        public bool isConnected => Client.Connected;
        public DateTime initDateTime => InitTime;

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
                return String.Empty;
            }

            //Preventing "System.Private.CoreLib:Index and count must refer to a location within the string. (Parameter 'count')" exception
            string ret = response.ToString();

            if (ret.Length > 5)
                return ret.Remove(0, 5);
            else
                return String.Empty;
        }

        public async Task DisconnectAsync() => await Client.DisconnectAsync(false);

        public void Dispose()
        {
            Client.Dispose();
            Disposed = true;
        }
    }
}
