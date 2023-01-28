using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.Network
{
    public class HandshakePacket : IPacket
    {
        private List<byte> data = new List<byte>
        {
            0x0//handshake packet id
        };
        private IPAddress ip { get; set; }
        private int protocolVersion { get; set; }
        private ushort port { get; set; }

        public HandshakePacket(IPAddress ip, int protocolVersion, ushort port)
        { 
            this.ip = ip;
            this.protocolVersion = protocolVersion;
            this.port = port;
        }

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
