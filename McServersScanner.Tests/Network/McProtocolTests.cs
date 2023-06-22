using McServersScanner.Core.Network;

namespace McServersScanner.Tests.Network
{
    [TestFixture]
    public class McProtocolTests
    {
        [Test]
        public void WriteReadVarIntTest()
        {
            const int expectedValue = 25565;

            byte[] valueBytes = McProtocol.WriteVarInt(expectedValue).ToArray();

            int value = McProtocol.ReadVarInt(new MemoryStream(valueBytes));

            Assert.That(value, Is.EqualTo(expectedValue));
        }
    }
}
