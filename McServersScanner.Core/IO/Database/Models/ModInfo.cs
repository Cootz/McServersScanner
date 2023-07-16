using System.Text.Json.Serialization;
using Realms;

namespace McServersScanner.Core.IO.Database.Models
{
    public class ModInfo : EmbeddedObject
    {
        [JsonPropertyName("modid")] public string ModId { get; init; }

        [JsonPropertyName("version")] public string Version { get; init; }

        public ModInfo()
        {
        }

        public ModInfo(string modId, string version)
        {
            ModId = modId;
            Version = version;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ModInfo modInfo && Equals(modInfo);
        }

        public bool Equals(ModInfo other) =>
            ModId == other.ModId
            && Version == other.Version;

        public override int GetHashCode() => HashCode.Combine(ModId, Version);
    }
}