using Realms;

namespace McServersScanner.IO.DB;

public class DBController
{
    public static readonly string DBPath = Path.Combine(Path_to_folder!, "servers.realm");

    public static string Path_to_folder => AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// Realm instance
    /// </summary>
    private Realm realm;

    public DBController()
    {
        var config = new RealmConfiguration(DBPath);

        realm = Realm.GetInstance(config);
    }

    /// <summary>
    /// Adds or updates new <see cref="ServerInfo"/> to database 
    /// </summary>
    public async Task AddOrUpdate(ServerInfo serverInfo)
    {
        if (serverInfo is not null)
        {
            await realm.WriteAsync(() =>
            {
                realm.Add(serverInfo, update: true);
            });
        }
    }

    public void RealmQuerry(Action<Realm> action) => action(realm);
}
