using System.Diagnostics;
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
        public void ForgeJsonToServerInfoTest()
        {
            const string json =
                @"{""description"":""A Minecraft Server"",""players"":{""max"":20,""online"":0},""version"":{""name"":""cauldron,craftbukkit,mcpc,fml,forge 1.7.10"",""protocol"":5},""modinfo"":{""type"":""FML"",""modList"":[{""modid"":""mcp"",""version"":""9.05""},{""modid"":""FML"",""version"":""7.10.114.1388""},{""modid"":""Forge"",""version"":""10.13.3.1388""},{""modid"":""BuildMod"",""version"":""v1.0""},{""modid"":""CoroAI"",""version"":""v1.0""},{""modid"":""ExtendedRenderer"",""version"":""v1.0""},{""modid"":""ConfigMod"",""version"":""v1.0""},{""modid"":""CustomSpawner"",""version"":""3.3.0""},{""modid"":""customnpcs"",""version"":""1.7.10c""},{""modid"":""PTRModelLib"",""version"":""1.0.0""},{""modid"":""props"",""version"":""2.4.1""},{""modid"":""MoCreatures"",""version"":""6.3.1""},{""modid"":""GollumCoreLib"",""version"":""2.0.0""},{""modid"":""MorePistons"",""version"":""1.7.10-1.5.1.3""},{""modid"":""TooMuchTime"",""version"":""2.4.0""},{""modid"":""TwilightForest"",""version"":""2.3.5""},{""modid"":""ZAMod"",""version"":""v1.9.5""}]}}";

            const string ip = "172.0.0.1";

            ServerInfo serverInfo = new(json, ip);

            Debug.Assert(serverInfo.ToString() == "");
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
