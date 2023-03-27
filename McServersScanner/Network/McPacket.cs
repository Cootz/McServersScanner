using System.Collections;

namespace McServersScanner.Network
{
    public class McPacket<T> : IEnumerable<byte> where T : IPacket 
    {
        private List<byte> Packet = new List<byte>();

        public McPacket(T? packet)
        {
            //Wrapping data to packet
            byte[] data = packet?.GetData() ?? new byte[0];

            Packet.AddRange(McProtocol.WriteVarInt(data.Length));
            Packet.AddRange(data);
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return Packet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Packet.GetEnumerator();
        }
    }
}
