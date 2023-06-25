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

        /// <summary>
        /// Test if dynamic json with missing properties is parsed correctly
        /// </summary>
        [Test(Description = "Test if dynamic json with missing properties is parsed correctly")]
        public void MinimalJsonToServerInfoTest()
        {
            ServerInfo serverInfo = new(ServerInfoDataSource.MINIMAL_JSON_SERVER_INFO,
                ServerInfoDataSource.SERVER_INFO_IP);

            Assert.That(serverInfo.Equals(ServerInfoDataSource.TestMinimalIServerInfo), Is.True);
        }

        [Test]
        public void EquivalentTest()
        {
            ServerInfo serverInfo = new(ServerInfoDataSource.JSON_SERVER_INFO, ServerInfoDataSource.SERVER_INFO_IP);

            bool equal = (serverInfo as IServerInfo).Equals(ServerInfoDataSource.TestServerInfo);

            Assert.That(equal, Is.True);
        }
    }
}
