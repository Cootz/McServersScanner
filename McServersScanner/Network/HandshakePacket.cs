using System.Net;

namespace McServersScanner.Network
{
    public class HandshakePacket : IPacket
    {
        /// <summary>
        /// Tcp data with packet id
        /// </summary>
        private List<byte> data = new List<byte>
        {
            0x0//handshake packet id
        };

        /// <summary>
        /// Ip of the target server
        /// </summary>
        private IPAddress ip { get; set; }

        /// <summary>
        /// Version of minecraft <see href="https://wiki.vg/Protocol_version_numbers#Versions_after_the_Netty_rewrite">protocol</see>
        /// </summary>
        private int protocolVersion { get; set; }

        /// <summary>
        /// Port of the target server
        /// </summary>
        private ushort port { get; set; }

        public HandshakePacket(IPAddress ip, int protocolVersion, ushort port)
        { 
            this.ip = ip;
            this.protocolVersion = protocolVersion;
            this.port = port;
        }

        /// <summary>
        /// Generates handshake packet data
        /// </summary>
        /// <returns>Handshake packet data</returns>
        public byte[] GetData()
        {
            //Creating handshake data
            int nextState = 1;
            
            data.AddRange(McProtocol.WriteVarInt(protocolVersion));//Version
            data.AddRange(McProtocol.WriteString(ip.ToString()));//Ip
            data.AddRange(BitConverter.GetBytes(port));//Port
            data.AddRange(McProtocol.WriteVarInt(nextState));//Next state

            return data.ToArray();
        }
    }
}
