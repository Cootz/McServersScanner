using McServersScanner.Core.Network;

namespace McServersScanner.Tests.Network
{
    [TestFixture]
    public class McClientTests
    {
        [Test]
        public async Task GetServerDataTest()
        {
            const string targetIp = "87.98.151.173";

            McClient client = new(targetIp, 25565)
            {
                BandwidthLimit = 1000
            };

            IAsyncResult connect = client.BeginConnect();

            connect.AsyncWaitHandle.WaitOne(2000);

            if (!client.IsConnected)
            {
                Assert.Warn("Client couldn't connect to {0}", targetIp);
                return;
            }

            string jsonServerInfo = await client.GetServerInfo();

            Console.WriteLine(jsonServerInfo);

            Assert.That(jsonServerInfo, Is.Not.EqualTo(string.Empty));
        }
    }
}
