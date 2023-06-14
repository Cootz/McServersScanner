using System.Diagnostics;
using McServersScanner.Core.Network;

namespace McServersScanner.Tests.Network
{
    [TestFixture]
    public class McClientTests
    {
        [Test, Timeout(2000)]
        public async Task GetServerDataTest()
        {
            McClient client = new("87.98.151.173", 25565, 1000);

            IAsyncResult connect = client.BeginConnect();

            connect.AsyncWaitHandle.WaitOne();

            string jsonServerInfo = await client.GetServerInfo();

            Console.WriteLine(jsonServerInfo);

            Assert.That(jsonServerInfo, Is.Not.EqualTo(string.Empty));
        }
    }
}
