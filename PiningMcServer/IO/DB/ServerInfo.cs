using CommunityToolkit.HighPerformance.Buffers;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Realms;

namespace McServersScanner.IO.DB
{

    /// <summary>
    /// Store info about minecraft server
    /// </summary>
    public class ServerInfo : RealmObject
    {
        [PrimaryKey]
        /// <summary>
        /// Target server ip
        /// </summary>
        public string IP { get; private set; } = string.Empty;

        /// <summary>
        /// Version info
        /// </summary>
        public Version Version { get; set; } = null!;

        /// <summary>
        /// Amount of players online
        /// </summary>
        public int? Online { get; set; } = null;

        /// <summary>
        /// Server description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Received answer
        /// </summary>
        public string JsonInfo { get; set; } = string.Empty;

        private ServerInfo() { }

        public ServerInfo(string jsonInfo, string ip)
        {
            IP = ip;

            JObject serverInfo = JObject.Parse(jsonInfo);

            JObject jVersion = (JObject)serverInfo["version"]!;

            Version = jVersion.ToObject<Version>()!;

            Online = serverInfo?["players"]?["online"]?.Value<int?>();

            Description = serverInfo?["description"]?["text"]?.Value<string>() ?? string.Empty;

            JsonInfo = jsonInfo;
        }
    }
}
