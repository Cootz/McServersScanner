namespace McServersScanner.Core.IO.Database.Models;

public interface IServerInfo : IEquatable<IServerInfo>
{
    /// <summary>
    /// Target server ip
    /// </summary>
    string Ip { get; }

    /// <summary>
    /// Version info
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Amount of players online
    /// </summary>
    int? Online { get; }

    /// <summary>
    /// Server description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// List of server mods
    /// </summary>
    public IList<ModInfo>? ModList { get; }

    /// <summary>
    /// Received answer
    /// </summary>
    string JsonInfo { get; }
}