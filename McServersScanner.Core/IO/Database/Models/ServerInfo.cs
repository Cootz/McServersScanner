using System.Text.Json;
using Realms;

namespace McServersScanner.Core.IO.Database.Models;

/// <summary>
/// Store info about minecraft server
/// </summary>
public class ServerInfo : RealmObject, IServerInfo
{
    [MapTo("IP")] [PrimaryKey] public string Ip { get; internal set; }

    public Version Version { get; set; }

    public int? Online { get; set; }

    public string Description { get; set; }

    public string JsonInfo { get; set; }

    private ServerInfo()
    {
    }

    public ServerInfo(string jsonInfo, string ip)
    {
        Ip = ip;

        JsonDocument serverInfo = JsonDocument.Parse(jsonInfo);

        JsonElement jVersion = serverInfo.RootElement.GetProperty("version");

        Version = jVersion.Deserialize<Version>()!;

        Online = serverInfo.RootElement.GetProperty("players").GetProperty("online").GetInt32();

        try
        {
            Description = serverInfo.RootElement.GetProperty("description").GetProperty("text").GetString()
                          ?? string.Empty;
        }
        catch (KeyNotFoundException)
        {
            Description = string.Empty;
        }

        JsonInfo = jsonInfo;
    }

    public bool Equals(IServerInfo? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Ip == other.Ip
               && Version.Equals(other.Version)
               && Online == other.Online
               && Description == other.Description
               && JsonInfo == other.JsonInfo;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ServerInfo)obj);
    }

    public override int GetHashCode() =>
        HashCode.Combine(base.GetHashCode(), Ip, Version, Online, Description, JsonInfo);
}