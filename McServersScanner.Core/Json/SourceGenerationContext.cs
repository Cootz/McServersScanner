using McServersScanner.Core.IO.Database.Models;
using System.Text.Json.Serialization;

namespace McServersScanner.Core.Json
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ServerInfo))]
    internal partial class SourceGenerationContext : JsonSerializerContext 
    {
    }
}
