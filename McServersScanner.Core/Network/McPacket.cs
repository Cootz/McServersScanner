using System.Collections;

namespace McServersScanner.Core.Network;

public class McPacket<T> : IEnumerable<byte> where T : IPacket
{
    private List<byte> packet = new();

    public McPacket(T? packet)
    {
        //Wrapping data to packet
        byte[] data = packet?.GetData() ?? Array.Empty<byte>();

        this.packet.AddRange(McProtocol.WriteVarInt(data.Length));
        this.packet.AddRange(data);
    }

    public IEnumerator<byte> GetEnumerator() => packet.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => packet.GetEnumerator();
}