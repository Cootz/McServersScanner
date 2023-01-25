using McServersScanner.IO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.DB;

public class DBController : DbContext
{
    private static readonly string DBPath = "Psw.db";
    private static readonly string Connection = $"Filename=\"{DBPath}\"";

    public DbSet<ServerInfo> ServerInfos { get; set; }

    public async Task Initialize()
    {
        await Database.EnsureCreatedAsync();
    }

    public async Task Add(ServerInfo serverInfo)
    {
        if (await ServerInfos.Where(x => x.IP == serverInfo.IP).CountAsync() == 0)
            await ServerInfos.AddAsync(serverInfo);
        else
            (await ServerInfos.Where(x => x.IP == serverInfo.IP).FirstAsync()).JsonInfo = serverInfo.JsonInfo;
        await SaveChangesAsync();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(Connection);
    }
}
