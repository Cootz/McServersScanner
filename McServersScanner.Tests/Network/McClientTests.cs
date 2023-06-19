using McServersScanner.Core;
using McServersScanner.Core.IO;
using McServersScanner.Core.Network;
using NSubstitute;

namespace McServersScanner.Tests.Network
{
    [TestFixture]
    public class McClientTests
    {
        [Test]
        public async Task GetServerDataTest()
        {
            const string targetIp = "87.98.151.173";

            IServiceProvider services = Substitute.For<IServiceProvider>();

            services.GetService(Arg.Any<Type>())
                .ReturnsForAnyArgs(new ThrottleManager(ScannerBuilder.DEFAULT_BANDWIDTH_LIMIT));

            McClient client = new(targetIp, 25565, services)
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
