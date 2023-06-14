using System.Net;

namespace McServersScanner.Core.Network;

public class HandshakePacket : McPacket
{
    public override int PacketId
    {
        get => 0;
    }

    protected override List<byte> data
    {
        get
        {
            //Creating handshake data
            const int nextState = 1;

            data.AddRange(McProtocol.WriteVarInt(protocolVersion)); //Version
            data.AddRange(McProtocol.WriteString(ip.ToString())); //Ip
            data.AddRange(BitConverter.GetBytes(port)); //Port
            data.AddRange(McProtocol.WriteVarInt(nextState)); //Next state

            return data;
        }
    }

    /// <summary>
    /// Ip of the target server
    /// </summary>
    private IPAddress ip { get; }

    /// <summary>
    /// Version of minecraft <see href="https://wiki.vg/Protocol_version_numbers#Versions_after_the_Netty_rewrite">protocol</see>
    /// </summary>
    private int protocolVersion { get; }

    /// <summary>
    /// Port of the target server
    /// </summary>
    private ushort port { get; }

    public HandshakePacket(IPAddress ip, int protocolVersion, ushort port)
    {
        this.ip = ip;
        this.protocolVersion = protocolVersion;
        this.port = port;
    }
}