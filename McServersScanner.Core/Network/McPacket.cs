using System.Collections;

namespace McServersScanner.Core.Network;

public class McPacket<T> : IEnumerable<byte> where T : IPacket
{
    private List<byte> Packet = new();

    public McPacket(T? packet)
    {
        //Wrapping data to packet
        byte[] data = packet?.GetData() ?? Array.Empty<byte>();

        Packet.AddRange(McProtocol.WriteVarInt(data.Length));
        Packet.AddRange(data);
    }

    public IEnumerator<byte> GetEnumerator() => Packet.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Packet.GetEnumerator();
}