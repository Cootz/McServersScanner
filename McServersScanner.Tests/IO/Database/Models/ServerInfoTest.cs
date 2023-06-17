using McServersScanner.Core.IO.Database.Models;
using McServersScanner.Tests.TestData;

namespace McServersScanner.Tests.IO.Database.Models
{
    [TestFixture]
    public class ServerInfoTest
    {
        [Test]
        public void JsonToServerInfoTest()
        {
            ServerInfo serverInfo = new(ServerInfoDataSource.JSON_SERVER_INFO, ServerInfoDataSource.SERVER_INFO_IP);

            Assert.That(serverInfo.Equals(ServerInfoDataSource.TestIServerInfo), Is.True);
        }

        [Test]
        public void EquivalentTest()
        {
            ServerInfo serverInfo = new(ServerInfoDataSource.JSON_SERVER_INFO, ServerInfoDataSource.SERVER_INFO_IP);

            bool equal = (serverInfo as IServerInfo).Equals((IServerInfo)ServerInfoDataSource.TestServerInfo);

            Assert.That(equal, Is.True);
        }
    }
}
