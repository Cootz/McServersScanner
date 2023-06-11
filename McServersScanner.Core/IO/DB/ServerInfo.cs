using System.Text.Json;
using Realms;

namespace McServersScanner.Core.IO.DB;

/// <summary>
/// Store info about minecraft server
/// </summary>
public class ServerInfo : RealmObject
{
    /// <summary>
    /// Target server ip
    /// </summary>
    [PrimaryKey]
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

    private ServerInfo()
    {
    }

    public ServerInfo(string jsonInfo, string ip)
    {
        IP = ip;

        JsonDocument serverInfo = JsonDocument.Parse(jsonInfo);

        JsonElement jVersion = serverInfo.RootElement.GetProperty("version");

        Version = jVersion.Deserialize<Version>()!;

        Online = serverInfo.RootElement.GetProperty("players").GetProperty("online").GetInt32();

        Description = serverInfo.RootElement.GetProperty("description").GetProperty("text").GetString() ?? string.Empty;

        JsonInfo = jsonInfo;
    }
}