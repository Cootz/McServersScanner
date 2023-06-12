using McServersScanner.Core.IO.Database.Models;
using Realms;

namespace McServersScanner.Core.IO.Database;

/// <summary>
/// Provides access to database
/// </summary>
public class DatabaseController : IDatabaseController
{
    public static readonly string DatabasePath = Path.Combine(PathToFolder!, "servers.realm");

    public static string PathToFolder
    {
        get => AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <summary>
    /// Realm instance
    /// </summary>
    private readonly Realm realm;

    public DatabaseController()
    {
        RealmConfiguration config = new(DatabasePath);

        realm = Realm.GetInstance(config);
    }

    public IQueryable<T> Select<T>() where T : RealmObject => realm.All<T>();

    public async Task Add(ServerInfo serverInfo) =>
        await realm.WriteAsync(() => { realm.Add(serverInfo, true); });

    public Task Update(ServerInfo serverInfo) => throw new NotImplementedException();

    public Task Delete(ServerInfo serverInfo) => throw new NotImplementedException();

    public void RealmQuery(Action<Realm> action) => action(realm);
}