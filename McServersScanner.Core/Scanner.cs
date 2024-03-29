﻿using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks.Dataflow;
using CommunityToolkit.HighPerformance.Buffers;
using McServersScanner.Core.IO.Database;
using McServersScanner.Core.IO.Database.Models;
using McServersScanner.Core.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McServersScanner.Core;

/// <summary>
/// Contains scanning logic of the app
/// </summary>
/// <remarks>
/// Only one instance of this class can exist in application
/// </remarks>
public sealed class Scanner : IScanner
{
    /// <summary>
    /// Maximum number of connections available at the same time
    /// </summary>
    public int ConnectionLimit { get; internal init; }

    /// <summary>
    /// Maximum number of bytes scanner can send/receive by network per second
    /// </summary>
    public int BandwidthLimit { get; internal init; }

    /// <summary>
    /// Exit database thread if true
    /// </summary>
    private bool endDBThread;

    /// <summary>
    /// Force running scanner to stop
    /// </summary>
    private bool forceStop;

    public BufferBlock<IPAddress> Ips
    {
        get => ips;
    }

    private readonly IServiceProvider services;

    /// <summary>
    /// Block of Ips to scan
    /// </summary>
    private readonly BufferBlock<IPAddress> ips;

    /// <summary>
    /// Array of ports to scan
    /// </summary>
    public ushort[] Ports { get; internal init; } = null!;

    /// <summary>
    /// Block of information about scanned servers
    /// </summary>
    private readonly BufferBlock<ServerInfo> serverInfos = new();

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public double Timeout { get; internal init; }

    /// <summary>
    /// List of clients
    /// </summary>
    private readonly ConcurrentDictionary<DateTime, McClient> clients = new();

    /// <summary>
    /// Supplies <see cref="ips"/> with <see cref="IPAddress"/>
    /// </summary>
    public Task AddIpAddresses { get; internal init; } = null!;

    /// <summary>
    /// The number of ips to scan
    /// </summary>
    public long TotalIps { get; internal init; }

    /// <summary>
    /// Amount of ips being scanned
    /// </summary>
    private long scannedIps;

    public StreamWriter OutputStream { get; internal init; } = null!;

    private readonly ILogger<Scanner>? logger;

    private readonly CancellationTokenSource databaseCancellationTokenSource = new();

    internal Scanner(BufferBlock<IPAddress> ips, IServiceProvider services)
    {
        this.ips = ips;
        updateDB = new Lazy<Thread>(() => new Thread(updateDatabase));
        this.services = services;

        logger = services.GetService<ILogger<Scanner>>();
    }

    /// <summary>
    /// Start scanning
    /// </summary>
    public async Task Scan()
    {
        ServicePointManager.DefaultConnectionLimit = ConnectionLimit;
        double currentRatio;

        //Starting update db thread
        updateDB.Value.Start();

        //Running workers
        Task writer = Task.Run(WriterAsync);
        Task reader = Task.Run(ReaderAsync);

        //Showing progress
        while (scannedIps < TotalIps)
        {
            if (forceStop)
                return;

            currentRatio = calculateRatio(scannedIps, TotalIps);

            OutputStream.Write("\r{0:0.00}% - {1}/{2}", currentRatio, scannedIps, TotalIps);

            await Task.Delay(100); //TODO: this is pretty weird way to show progress
        }

        currentRatio = calculateRatio(scannedIps, TotalIps);
        OutputStream.WriteLine("{0:0.00}% - {1}/{2}", currentRatio, scannedIps, TotalIps);
        await OutputStream.WriteLineAsync("Waiting for the results...");

        await Task.WhenAll(writer, reader, AddIpAddresses); //awaiting for results

        exitDatabase();

        updateDB.Value.Join();

        static double calculateRatio(double currentInQueue, double length) => currentInQueue / length * 100.0;
    }

    /// <summary>
    /// Looks <see cref="clients"/> for timed-out <see cref="McClient"/>s and remove them
    /// </summary>
    /// <remarks>
    /// This task runs in different thread
    /// </remarks>
    public async Task ReaderAsync()
    {
        TimeSpan timeToConnect = TimeSpan.FromSeconds(Timeout);

        do
        {
            if (!clients.IsEmpty)
            {
                IEnumerable<KeyValuePair<DateTime, McClient>> timeoutClients =
                    from c in clients where DateTime.Now - c.Key > timeToConnect && !c.Value.IsConnected select c;

                foreach ((DateTime startTime, McClient? client) in timeoutClients)
                {
                    if (forceStop)
                        return;

                    if (!client.Disposed) client.Dispose();

                    clients.TryRemove(startTime, out _);
                    scannedIps++;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        } while (TotalIps - scannedIps > 0);
    }

    /// <summary>
    /// Create new <see cref="McClient"/>, start and add it to <see cref="clients"/>
    /// </summary>
    /// <remarks>
    /// This task runs in different thread
    /// </remarks>
    public async Task WriterAsync()
    {
        long addedIps = 0;

        while (TotalIps - addedIps > 0)
            foreach (ushort port in Ports)
            {
                if (forceStop)
                    return;

                while (clients.Count >= ConnectionLimit) await Task.Delay(50);

                McClient client = new(await ips.ReceiveAsync(), port, services)
                {
                    ConnectionCallBack = OnConnected,
                    BandwidthLimit = BandwidthLimit / ConnectionLimit
                };

                try
                {
                    client.BeginConnect();
                    clients.TryAdd(DateTime.Now, client);
                    addedIps++;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Cannot connect to {ip_address}", client.IpEndPoint);
                }
            }
    }

    /// <summary>
    /// Callback for <see cref="McClient.ConnectionCallBack"/>. Sends server info and receives answer
    /// </summary>
    public async void OnConnected(IAsyncResult result)
    {
        if (result.AsyncState is null)
            return;

        if (forceStop)
            return;

        McClient client = (McClient)result.AsyncState!;

        if (!client.IsConnected)
        {
            client.Dispose();
            return;
        }

        logger?.LogInformation("Successfully connected to {ip_address}", client.IpEndPoint);

        string data = await client.GetServerInfo();

        if (data.StartsWith('{'))
        {
            try
            {
                ServerInfo serverInfo = new(data, StringPool.Shared.GetOrAdd(client.IpEndPoint.Address.ToString()));
                serverInfos.Post(serverInfo);
                logger?.LogInformation("Successfully parsed {raw_data}", data);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Cannot parse data from {ip_address}: {raw_data}", client.IpEndPoint, data);
            }
        }

        try
        {
            await client.DisconnectAsync();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to disconnect {ip_address}", client.IpEndPoint);
        }

        client.Dispose();
    }

    /// <summary>
    /// Thread with database. Update database with scanned data
    /// </summary>
    private readonly Lazy<Thread> updateDB;

    private void updateDatabase()
    {
        DatabaseController database = new();

        while (!endDBThread)
        {
            try
            {
                ServerInfo info = serverInfos.ReceiveAsync(databaseCancellationTokenSource.Token).Result;

                database.Add(info).Wait();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Database thread has thrown an exception");
            }
        }
    }

    private void exitDatabase()
    {
        endDBThread = true;
        databaseCancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Asynchronously copy data from enumerable to buffer block
    /// </summary>
    /// <typeparam name="T">Type of class to copy</typeparam>
    /// <param name="typeEnumerable">Copy from</param>
    /// <param name="bufferBlock">Copy to</param>
    public static async Task CopyToBufferBlockAsync<T>(IEnumerable<T> typeEnumerable, BufferBlock<T> bufferBlock)
        where T : class
    {
        foreach (T item in typeEnumerable)
            await bufferBlock.SendAsync(item);
    }

    /// <summary>
    /// Force all running tasks and threads to stop
    /// </summary>
    public void ForceStop()
    {
        forceStop = true;

        exitDatabase();

        logger?.LogInformation("\nStopping application...");
        OutputStream.WriteLine("\nStopping application...");

        updateDB.Value.Join();
    }
}