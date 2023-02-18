﻿using Microsoft.EntityFrameworkCore;

namespace McServersScanner.IO.DB;

public class DBController : DbContext
{
    private static readonly string DBPath = "Psw.db";
    private static readonly string Connection = $"Filename=\"{DBPath}\"";

    public DbSet<ServerInfo> ServerInfos { get; set; }

    public async Task Initialize()
    {
        await Database.EnsureCreatedAsync();
        await Database.OpenConnectionAsync();
        await ServerInfos.LoadAsync();
    }

    public async Task AddOrUpdate(ServerInfo serverInfo)
    {
        if (serverInfo is not null)
        {
            await ServerInfos.AddAsync(serverInfo);
        }            
    }

    public void Update(ServerInfo updatedInfo)
    {
        Set<ServerInfo>().Attach(updatedInfo);
        ServerInfos.Update(updatedInfo);
        Entry(updatedInfo).State = EntityState.Modified;
        SaveChanges();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(Connection);
    }
}
