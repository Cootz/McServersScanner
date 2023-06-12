using System.Text.Json.Serialization;
using Realms;

namespace McServersScanner.Core.IO.Database;

public class Version : EmbeddedObject
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("protocol")] public int? Protocol { get; set; } = null;

    public override string ToString() => $"Name: {Name}, protocol {Protocol}";
}