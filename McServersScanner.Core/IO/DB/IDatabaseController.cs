using Realms;

namespace McServersScanner.Core.IO.DB;

/// <summary>
/// Provides access to database
/// </summary>
public interface IDatabaseController
{
    /// <summary>
    /// Adds new <see cref="ServerInfo"/> to database 
    /// </summary>
    Task Add(ServerInfo serverInfo);

    /// <summary>
    /// Updates <see cref="ServerInfo"/> value in database 
    /// </summary>
    Task Update(ServerInfo serverInfo);

    /// <summary>
    /// Removes <see cref="ServerInfo"/> from database 
    /// </summary>
    Task Delete(ServerInfo serverInfo);

    /// <summary>
    /// A direct query to existing realm
    /// </summary>
    void RealmQuery(Action<Realm> action);
}