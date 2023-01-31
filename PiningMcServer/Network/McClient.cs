using System.Net;
using System.Net.Sockets;
using System.Text;

namespace McServersScanner.Network
{
    public class McClient : IDisposable
    {
        private IPEndPoint iPEndPoint { get; set; }
        private Socket Client { get; set; }

        public McClient(string ip, ushort port) : this(IPAddress.Parse(ip), port) { }

        public McClient(IPAddress ip, ushort port)
        {
            iPEndPoint = new IPEndPoint(ip, port);
            Client = new(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task<string> GetServerInfo(int timeout)
        {
            int protocolVersion = 761;
            StringBuilder response = new StringBuilder();

            try
            {
                var result = Client.BeginConnect(iPEndPoint, null, null);
                var success = result.AsyncWaitHandle.WaitOne(timeout, true); //Waiting connection for a timeout

                if (!success)//Stop exec if not connected
                {
                    Client.Close();
                    return String.Empty;
                }

                Client.ReceiveTimeout = timeout / 2;
                Client.SendTimeout = timeout / 2;

                McPacket<HandshakePacket> packet = new (new HandshakePacket(iPEndPoint.Address, protocolVersion, (ushort)(iPEndPoint.Port)));

                //Send handshake
                await Client.SendAsync(packet.ToArray(), SocketFlags.None);

                //Send ping req
                byte[] pingData = { 1, 0 };
                await Client.SendAsync(pingData, SocketFlags.None);

                byte[] buffer = new byte[1024];
                int bytesReceived = 0;

                do
                {
                    bytesReceived = await Client.ReceiveAsync(buffer, SocketFlags.None);
                    response.Append(Encoding.UTF8.GetString(buffer));
                } while (bytesReceived > 0);
            }
            catch
            {
                Client.Close();
                return String.Empty;
            }



            //Preventing "System.Private.CoreLib:Index and count must refer to a location within the string. (Parameter 'count')" exception
            string ret = response.ToString();

            if (ret.Length > 5)
                return ret.Remove(0, 5);
            else
                return String.Empty;
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
