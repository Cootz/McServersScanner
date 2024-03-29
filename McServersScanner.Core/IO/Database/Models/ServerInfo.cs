﻿using System.Text.Json;
using static McServersScanner.Core.Utils.JsonHelper;
using Realms;

namespace McServersScanner.Core.IO.Database.Models;

/// <summary>
/// Store info about minecraft server
/// </summary>
public class ServerInfo : RealmObject, IServerInfo
{
    [MapTo("IP")] [PrimaryKey] public string Ip { get; internal set; } = string.Empty;

    public Version Version { get; set; } = null!;

    public int? Online { get; set; }

    public string Description { get; set; } = null!;

    public IList<ModInfo>? ModList { get; }

    public string JsonInfo { get; set; } = null!;

    private ServerInfo()
    {
    }

    public ServerInfo(string jsonInfo, string ip)
    {
        Ip = ip;

        JsonDocument serverInfo = JsonDocument.Parse(jsonInfo);

        JsonElement jVersion = serverInfo.RootElement.GetProperty("version");

        Version = jVersion.Deserialize<Version>()!;

        Online = serverInfo.RootElement.Get("players")?.Get("online")?.GetInt32();

        JsonElement? descriptionElement = serverInfo.RootElement.Get("description");

        if (descriptionElement?.ValueKind == JsonValueKind.String)
            Description = descriptionElement?.GetString() ?? string.Empty;
        else
            Description = descriptionElement?.Get("text")?.GetString() ?? string.Empty;

        JsonElement? modList = serverInfo.RootElement.Get("modinfo")?.Get("modList");

        if (modList.HasValue)
        {
            ModList = JsonSerializer.Deserialize<List<ModInfo>>(modList.Value.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = false });
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
               && JsonInfo == other.JsonInfo
               && ((ModList is not null && other.ModList is not null)
                   && (ModList.Count == other.ModList.Count
                       && ModList.All(other.ModList.Contains)));
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ServerInfo)obj);
    }

    public override int GetHashCode() =>
        HashCode.Combine(Ip, Version, Online, Description, JsonInfo);
}