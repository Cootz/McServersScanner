using McServersScanner.Core.IO.Database.Models;
using NSubstitute;
using Version = McServersScanner.Core.IO.Database.Models.Version;

namespace McServersScanner.Tests.TestData
{
    public static class ServerInfoDataSource
    {
        public const string JSON_SERVER_INFO =
            @"{
                ""address"": ""awesomeserver.example.com"",
                ""online"": true,
                ""description"": {
                    ""text"": ""A Minecraft Server""
                },
                ""favicon"": ""data:image/png;base64.... big blob of bytes..."",
                ""version"": {
                    ""name"": ""1.14.4"",
                    ""protocol"": 498
                },
                ""players"": {
                    ""online"": 1,
                    ""max"": 20
                }
            }";

        public const string SERVER_INFO_IP = "10.0.0.5";

        public static readonly IServerInfo TestIServerInfo =
            Substitute.For<IServerInfo>()
                .Configure(s =>
                {
                    s.Ip.Returns(SERVER_INFO_IP);
                    s.Version.Returns(new Version
                    {
                        Name = "1.14.4",
                        Protocol = 498
                    });
                    s.Online.Returns(1);
                    s.Description.Returns("A Minecraft Server");
                    s.JsonInfo.Returns(JSON_SERVER_INFO);
                });

        /// <remarks>
        /// DO NOT add this instance into realm
        /// </remarks>
        public static readonly ServerInfo TestServerInfo = new(JSON_SERVER_INFO, SERVER_INFO_IP);

        public static T Configure<T>(this T value, Action<T> action)
        {
            action(value);
            return value;
        }
    }
}