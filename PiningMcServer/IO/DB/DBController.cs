using Microsoft.EntityFrameworkCore;
using Realms;

namespace McServersScanner.IO.DB;

public class DBController
{
    private static readonly string DBPath = "Psw.db";

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
}
