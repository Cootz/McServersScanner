using McServersScanner.Core.IO.Database.Models;
using McServersScanner.Tests.TestData;
using NSubstitute;

namespace McServersScanner.Tests.IO.Database.Models
{
    [TestFixture]
    public class ServerInfoTest
    {
        [Test]
        public void JsonToServerInfoTest()
        {
            ServerInfo serverInfo = new(ServerInfoDataSource.GetJsonServerInfo(), ServerInfoDataSource.SERVER_INFO_IP);

            Assert.That(serverInfo.Equals(ServerInfoDataSource.TestIServerInfo), Is.True);
        }

        [Test]
        public void JsonToServerInfoWithUnwrappedDescriptionTest()
        {
            ServerInfo serverInfo = new(ServerInfoDataSource.GetJsonServerInfo(true),
                ServerInfoDataSource.SERVER_INFO_IP);

            IServerInfo iServerInfo = ServerInfoDataSource.TestIServerInfo;

            iServerInfo.JsonInfo.Returns(ServerInfoDataSource.GetJsonServerInfo(true));

            Assert.That(serverInfo.Equals(iServerInfo), Is.True);
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

        [TestCaseSource(typeof(ServerInfoDataSource), nameof(ServerInfoDataSource.PositiveEqualTestCases))]
        public bool Equivalent_WithDifferentValues_ReturnsAsExpected(IServerInfo first, IServerInfo second) =>
            first.Equals(second);
    }
}
