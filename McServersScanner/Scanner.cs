﻿using CommunityToolkit.HighPerformance.Buffers;
using McServersScanner.IO.DB;
using McServersScanner.Network;
using McServersScanner.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks.Dataflow;

namespace McServersScanner
{
    /// <summary>
    /// Contains scanning logic of the app
    /// </summary>
    /// <remarks>
    /// Only one instance of this class can exist in application
    /// </remarks>
    public static class Scanner
    {
        /// <summary>
        /// Number of ports to scan
        /// </summary>
        public static int PortsCount => ports.Length;

        /// <summary>
        /// Maximum number of connections available at the same time
        /// </summary>
        public static int ConnectionLimit => connectionLimit;

        /// <summary>
        /// Exit database thread if true
        /// </summary>
        private static bool endDBThread = false;

        /// <summary>
        /// Force running scanner to stop
        /// </summary>
        private static bool forceStop = false;

        /// <summary>
        /// Block of Ips to scan
        /// </summary>
        private static BufferBlock<IPAddress> ips = null!;

        /// <summary>
        /// Array of ports to scan
        /// </summary>
        private static ushort[] ports = new ushort[] { 25565 };

        /// <summary>
        /// Block of information about scanned servers
        /// </summary>
        private static BufferBlock<ServerInfo> serverInfos = new();

        /// <summary>
        /// Number of active connections that the app can support at the same time
        /// </summary>
        private static int connectionLimit = 1000;

        /// <summary>
        /// Connection timeout in seconds
        /// </summary>
        private static double timeout = 10;

        /// <summary>
        /// List of clients
        /// </summary>
        private static ConcurrentDictionary<DateTime, McClient> clients = new();

        /// <summary>
        /// Supplies <see cref="ips" with <see cref="IPAddress"/>es/>
        /// </summary>
        private static Task addIpAdresses = Task.CompletedTask;

        /// <summary>
        /// The number of ips to scan
        /// </summary>
        private static long totalIps = 0;

        /// <summary>
        /// Amount of ips being scanned
        /// </summary>
        private static long scannedIps = 0;

        /// <summary>
        /// Applies scanner configuration
        /// </summary>
        public static void ApplyConfiguration(ScannerConfiguration configuration)
        {
            ips = configuration.ips;
            ports = configuration.ports ?? ports;
            connectionLimit = configuration.connectionLimit ?? connectionLimit;
            timeout = configuration.timeout ?? timeout;
            addIpAdresses = configuration.addIpAdresses ?? addIpAdresses;
            totalIps = configuration.totalIps ?? totalIps;
        }

        /// <summary>
        /// Start scanning
        /// </summary>
        public static async Task Scan()
        {
            ServicePointManager.DefaultConnectionLimit = connectionLimit;
            double currentRatio;

            //Starting update db thread
            updateDb.Start();

            //Running workers
            Task writer = Task.Run(WriterAsync);
            Task reader = Task.Run(ReaderAsync);

            //Showing progress
            while (scannedIps < totalIps)
            {
                if (forceStop)
                    return;

                currentRatio = CalculateRatio(scannedIps, totalIps);

                Console.Write("\r{0:0.00}% - {1}/{2}", currentRatio, scannedIps, totalIps);

                await Task.Delay(100);
            }

            currentRatio = CalculateRatio(scannedIps, totalIps);
            Console.WriteLine("{0:0.00}% - {1}/{2}", currentRatio, scannedIps, totalIps);
            Console.WriteLine("Waiting for the results...");

            await Task.WhenAll(writer, reader, addIpAdresses); //awaiting for results

            endDBThread = true;//exiting db thread
            updateDb.Join();
        }

        /// <summary>
        /// Looks <see cref="clients"/> for timeouted <see cref="McClient"/>s and remove them
        /// </summary>
        /// <remarks>
        /// This task runs in different thread
        /// </remarks>
        public static async Task ReaderAsync()
        {
            TimeSpan timeToConnect = TimeSpan.FromSeconds(timeout);

            do
            {
                if (!clients.IsEmpty)
                {
                    var timeoutClients = from c in clients where DateTime.Now - c.Key > timeToConnect select c;

                    foreach (var (startTime, client) in clients)
                    {
                        if (forceStop)
                            return;

                        if (!client.Disposed)
                        {
                            client.Dispose();
                        }

                        clients.TryRemove(startTime, out _);
                        scannedIps++;
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            } while ((totalIps - scannedIps) > 0);
        }

        /// <summary>
        /// Creates new <see cref="McClient"/>, starts and add it to <see cref="clients"/>
        /// </summary>
        /// <remarks>
        /// This task runs in different thread
        /// </remarks>
        public static async Task WriterAsync()
        {
            long addedIps = 0;

            while ((totalIps - addedIps) > 0)
            {
                foreach (ushort port in ports)
                {
                    if (forceStop)
                        return;

                    McClient client = new(await ips.ReceiveAsync(), port, OnConnected);

                    try
                    {
                        client.BeginConnect();
                        clients.TryAdd(DateTime.Now, client);
                        addedIps++;
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Callback for <see cref="McClient.connectionCallBack"/>. Sends server info and receives answer
        /// </summary>
        /// <param name="result"></param>
        public static async void OnConnected(IAsyncResult result)
        {
            if (result.AsyncState is null)
                return;

            if (forceStop)
                return;

            McClient client = (McClient)result.AsyncState!;

            //if (!client.isConnected)
            //{
            //    client.Dispose();
            //    return;
            //}

            string data = await client.GetServerInfo();

            if (data.StartsWith('{'))
            {
                try
                {
                    string jsonData = StringPool.Shared.GetOrAdd(JsonHelper.ConvertToJsonString(data));

                    ServerInfo serverInfo = new(jsonData, StringPool.Shared.GetOrAdd(client.IpEndPoint.Address.ToString()));
                    serverInfos.Post(serverInfo);
                }
                catch { }
            }

            try
            {
                await client.DisconnectAsync();
            }
            catch { }

            client.Dispose();
        }

        /// <summary>
        /// Thread with database. Update database with scanned data
        /// </summary>
        static Thread updateDb = new(() => UpdateDatabase());

        private static void UpdateDatabase()
        {
            DBController DB = new();

            while (!endDBThread)
            {
                int collectedInfosCount = serverInfos.Count;

                while (collectedInfosCount > 0)
                {
                    try
                    {
                        DB.AddOrUpdate(serverInfos.ReceiveAsync().Result).Wait();
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
        public static async Task CopyToActionBlockAsync<T>(IEnumerable<T> typeEnumerable, BufferBlock<T> typeActionBlock) where T : class
        {
            foreach (T item in typeEnumerable)
                await typeActionBlock.SendAsync(item);
        }

        /// <summary>
        /// Force all running tasks and threads to stop
        /// </summary>
        public static void ForceStop()
        {
            endDBThread = true;
            forceStop = true;

            Console.WriteLine("\nStopping application...");

            updateDb.Join();
        }

        /// <summary>
        /// Calculates percantage ratio of servers scanning progress
        /// </summary>
        /// <param name="currentInQueue">Current position in queue</param>
        /// <param name="length">Length of queue</param>
        /// <returns>Progress percentage rounded to 2 digits</returns>
        static double CalculateRatio(double currentInQueue, double length) => currentInQueue / length * 100.0;

        /// <summary>
        /// Resets scanner to default state.
        /// <para><b>FOR TESTING PURPOSE ONLY</b></para>
        /// </summary>
        public static void Reset()
        {
            endDBThread = false;
            forceStop = false;
            ips = null!;
            ports = new ushort[] { 25565 };
            serverInfos = new();
            connectionLimit = 1000;
            timeout = 10;
            clients = new();
            addIpAdresses = Task.CompletedTask;
            totalIps = 0;
            scannedIps = 0;

            updateDb = new(() => UpdateDatabase());
        }
    }
}
