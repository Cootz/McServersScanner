using Realms;

namespace McServersScanner.Core.IO.Database.Models
{
    public class ModInfo : EmbeddedObject
    {
        public string ModId { get; private set; }

        public string Version { get; private set; }

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
