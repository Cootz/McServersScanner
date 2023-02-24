using Realms;
using System.Text.Json.Serialization;

namespace McServersScanner.IO.DB
{
    public class Version : EmbeddedObject
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("protocol")]
        public int? Protocol { get; set; } = null;

        public override string ToString()
        {
            return $"Name: {Name}, protocol {Protocol}";
        }
    }
}
