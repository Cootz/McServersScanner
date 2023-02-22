using McServersScanner.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Realms;
using System.Text.Json.Serialization;

namespace McServersScanner.IO.DB
{

    /// <summary>
    /// Store info about minecraft server
    /// </summary>
    public class ServerInfo : RealmObject
    {
        [PrimaryKey]
        public int ID { get; set; }
     
        /// <summary>
        /// Target server ip
        /// </summary>
        public string IP { get; private set; } = string.Empty;

        /// <summary>
        /// Version info
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Amount of players online
        /// </summary>
        public int? Online { get; set; } = null;

        /// <summary>
        /// Received answer
        /// </summary>
        public string JsonInfo { get; set; } = string.Empty;

        private ServerInfo() { }

        public ServerInfo(string jsonInfo, string ip)
        {
            IP = ip;

            JObject serverInfo = JObject.Parse(JsonHelper.ConvertToJsonString(jsonInfo));

            Version = JsonConvert.DeserializeObject<Version>((string)serverInfo["version"]!)!;

            Online = (int?)serverInfo?["players"]?["online"];



            JsonInfo = jsonInfo;
        }
    }
}
