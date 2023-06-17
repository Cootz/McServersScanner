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
    Version Version { get; set; }

    /// <summary>
    /// Amount of players online
    /// </summary>
    int? Online { get; set; }

    /// <summary>
    /// Server description
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Received answer
    /// </summary>
    string JsonInfo { get; set; }
}