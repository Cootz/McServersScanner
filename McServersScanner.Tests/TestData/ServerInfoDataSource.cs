using McServersScanner.Core.IO.Database;

namespace McServersScanner.Tests.TestData
{
    public static class ServerInfoDataSource
    {
        public const string JSON_SERVER_INFO =
            @"{
                ""status"": ""success"",
                ""online"": true,
                ""motd"": ""Test MC | Let's build a new ..."",
                ""description"": {
                    ""extra"": [
                        {
                            ""color"": ""gold"",
                            ""text"": ""D""
                        },
                        {
                            ""color"": ""gold"",
                            ""text"": ""a""
                        }
                    ],
                    ""text"": """"
                },
                ""error"": null,
                ""players"": {
                    ""max"": 100,
                    ""online"": 9
                },
                ""version"": {
                    ""name"": ""Paper 1.18.2"",
                    ""protocol"": 758
                },
                ""last_updated"": ""1652491121"",
                ""duration"": ""24093588""
            }";

        public const string SERVER_INFO_IP = "10.0.0.5";

        public static readonly ServerInfo TestServerInfo = new(JSON_SERVER_INFO, SERVER_INFO_IP);
    }
}