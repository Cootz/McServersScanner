using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks.Dataflow;
using CommunityToolkit.HighPerformance.Buffers;
using McServersScanner.Core.IO.Database;
using McServersScanner.Core.IO.Database.Models;
using McServersScanner.Core.Network;
using McServersScanner.Core.Utils;

namespace McServersScanner.Core;

/// <summary>
/// Contains scanning logic of the app
/// </summary>
/// <remarks>
/// Only one instance of this class can exist in application
/// </remarks>
public sealed class Scanner : IScannerOptions
{
    /// <summary>
    /// Maximum number of connections available at the same time
    /// </summary>
    public int ConnectionLimit { get; internal init; }

    /// <summary>
    /// Maximum number of bytes scanner can sent/receive by network per second
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

    internal Scanner(BufferBlock<IPAddress> ips)
    {
        this.ips = ips;
        updateDB = new Lazy<Thread>(() => new Thread(updateDatabase));
    }

    /// <summary>
    /// Start scanning
    /// </summary>
    public async Task Scan()
    {
        _ = new ThrottleManager(BandwidthLimit);

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

            //TODO: This shouldn't be bound to Console class. Change to Logger or any type of writable stream 
            Console.Write("\r{0:0.00}% - {1}/{2}", currentRatio, scannedIps, TotalIps);

            await Task.Delay(100); //TODO: this is pretty weird way to show progress
        }

        currentRatio = calculateRatio(scannedIps, TotalIps);
        Console.WriteLine("{0:0.00}% - {1}/{2}", currentRatio, scannedIps, TotalIps);
        Console.WriteLine("Waiting for the results...");

        await Task.WhenAll(writer, reader, AddIpAddresses); //awaiting for results

        endDBThread = true; //exiting db thread
        updateDB.Value.Join();
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
                    from c in clients where DateTime.Now - c.Key > timeToConnect select c;

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
    /// Creates new <see cref="McClient"/>, starts and add it to <see cref="clients"/>
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

                McClient client = new(await ips.ReceiveAsync(), port)
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
                catch
                {
                    // TODO: Add logging
                }
            }
    }

    /// <summary>
    /// Callback for <see cref="McClient.ConnectionCallBack"/>. Sends server info and receives answer
    /// </summary>
    /// <param name="result"></param>
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

        string data = await client.GetServerInfo();

        if (data.StartsWith('{'))
            try
            {
                string jsonData = StringPool.Shared.GetOrAdd(JsonHelper.ConvertToJsonString(data));

                ServerInfo serverInfo = new(jsonData, StringPool.Shared.GetOrAdd(client.IpEndPoint.Address.ToString()));
                serverInfos.Post(serverInfo);
            }
            catch
            {
                // TODO: Add logging
            }

        try
        {
            await client.DisconnectAsync();
        }
        catch
        {
            // TODO: Add logging
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
            int collectedInfosCount = serverInfos.Count;

            while (collectedInfosCount > 0)
            {
                try
                {
                    database.Add(serverInfos.ReceiveAsync().Result).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0}: {1}; {2}", ex.Source, ex.Message, ex.InnerException?.Message ?? "");
                }

                collectedInfosCount--;
            }

            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// Asynchronously copy data from enumerable to actionBlock
    /// </summary>
    /// <typeparam name="T">Type of class to copy</typeparam>
    /// <param name="typeEnumerable">Copy from</param>
    /// <param name="typeActionBlock">Copy to</param>
    public static async Task CopyToActionBlockAsync<T>(IEnumerable<T> typeEnumerable, BufferBlock<T> typeActionBlock)
        where T : class
    {
        foreach (T item in typeEnumerable)
            await typeActionBlock.SendAsync(item);
    }

    /// <summary>
    /// Force all running tasks and threads to stop
    /// </summary>
    public void ForceStop()
    {
        endDBThread = true;
        forceStop = true;

        Console.WriteLine("\nStopping application...");

        updateDB.Value.Join();
    }

    /// <summary>
    /// Calculates percentage ratio of servers scanning progress
    /// </summary>
    /// <param name="currentInQueue">Current position in queue</param>
    /// <param name="length">Length of queue</param>
    /// <returns>Progress percentage rounded to 2 digits</returns>
    private double calculateRatio(double currentInQueue, double length) => currentInQueue / length * 100.0;
}