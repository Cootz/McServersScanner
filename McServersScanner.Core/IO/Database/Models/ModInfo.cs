using Realms;

namespace McServersScanner.Core.IO.Database.Models
{
    public class ModInfo : EmbeddedObject
    {
        public string ModId { get; init; }

        public string Version { get; init; }

        public ModInfo()
        {
        }

        public ModInfo(string modId, string version)
        {
            ModId = modId;
            Version = version;
        }
    }
}
