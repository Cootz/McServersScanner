using Realms;

namespace McServersScanner.Core.IO.DB;

/// <summary>
/// Provides access to database
/// </summary>
// ReSharper disable once InconsistentNaming
public class DBController
{
    public static readonly string DBPath = Path.Combine(PathToFolder!, "servers.realm");

    public static string PathToFolder
    {
        get => AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <summary>
    /// Realm instance
    /// </summary>
    private readonly Realm realm;

    public DBController()
    {
        RealmConfiguration? config = new RealmConfiguration(DBPath);

        realm = Realm.GetInstance(config);
    }

    /// <summary>
    /// Adds or updates new <see cref="ServerInfo"/> to database 
    /// </summary>
    public async Task AddOrUpdate(ServerInfo serverInfo)
    {
        if (serverInfo is not null)
            await realm.WriteAsync(() => { realm.Add(serverInfo, true); });
    }

    /// <summary>
    /// A direct query to existing realm
    /// </summary>
    public void RealmQuerry(Action<Realm> action) => action(realm);
}