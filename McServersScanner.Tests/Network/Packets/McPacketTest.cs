using McServersScanner.Core.Network;
using McServersScanner.Core.Network.Packets;

namespace McServersScanner.Tests.Network.Packets
{
    [TestFixture]
    public class McPacketTest
    {
        private const int packet_id = 1;

        private static readonly byte[] packetData = { 102, 143 };

        [Test]
        public void ConvertPacketToRawByteArray()
        {
            List<byte> expectedRawPacket = new();

            expectedRawPacket.AddRange(McProtocol.WriteVarInt(packet_id));
            expectedRawPacket.AddRange(packetData);
            expectedRawPacket.InsertRange(0, McProtocol.WriteVarInt(expectedRawPacket.Count));

            McPacketTestImpl mcPacket = new();

            List<byte> rawPacket = mcPacket.ToList();

            Assert.That(rawPacket, Is.EqualTo(expectedRawPacket));
        }

        private class McPacketTestImpl : McPacket
        {
            public override int PacketId
            {
                get => packet_id;
            }

            protected override List<byte> data
            {
                get => new(packetData);
            }
        }
    }
}