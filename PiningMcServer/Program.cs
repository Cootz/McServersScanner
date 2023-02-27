using CommandLine;
using CommunityToolkit.HighPerformance.Buffers;
using McServersScanner.CLI;
using McServersScanner.IO.DB;
using McServersScanner.Network;
using McServersScanner.Utils;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;

internal class Program
{
    /// <summary>
    /// Exit database thread if true
    /// </summary>
    private static bool endDBThread = false;

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
    /// Amout of active connections app can handle at the same time
    /// </summary>
    private static int connectionLimit = 10000;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    private static double timeout = 10;
    
    /// <summary>
    /// List of clients
    /// </summary>
    private static List<McClient> clients = new();

    /// <summary>
    /// Supplies <see cref="ips" with <see cref="IPAddress"/>es/>
    /// </summary>
    private static Task addIpAdresses = Task.CompletedTask;

    /// <summary>
    /// Amount of ips to scan
    /// </summary>
    private static long totalIps = 0;

    /// <summary>
    /// Amount of ips being scanned
    /// </summary>
    private static long scannedIps = 0;

    private static async Task Main(string[] args)
    {
        //Parsing cmd params
        ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                //Adding connection limit
                int? conLimit = o.ConnectionLimit;

                if (conLimit is not null)
                    connectionLimit = conLimit.Value;

                //Adding ports
                List<string>? portList = o.Ports?.ToList();

                if (portList is not null)
                {
                    List<ushort> portUshot = new();

                    foreach (string portString in portList)
                    {
                        if (portString.Contains('-')) //ports range
                        {
                            string[] splittedPorts = portString.Split('-');

                            var range = NetworkHelper.FillPortRange(splittedPorts[0], splittedPorts[1]);
                            foreach (ushort port in range)
                                portUshot.Add(port);
                        }
                        else //single port
                        {
                            ports = new ushort[] { ushort.Parse(portString) };
                        }
                    }
                }

                //Adding ips
                ips = new(new DataflowBlockOptions()
                {
                    BoundedCapacity = connectionLimit
                });

                List<string> ipOptionRange = o.Range!.ToList();

                foreach (string ipOption in ipOptionRange)
                {
                    if (ipOption.Contains('-')) //ip range
                    {
                        string[] splittedIps = ipOption.Split('-');

                        string firstIp = StringPool.Shared.GetOrAdd(splittedIps[0]);
                        string lastIp = StringPool.Shared.GetOrAdd(splittedIps[1]);

                        totalIps = NetworkHelper.GetIpRangeCount(firstIp, lastIp) * ports.Length;

                        var range = NetworkHelper.FillIpRange(firstIp, lastIp);

                        addIpAdresses = Task.Run(() => copyToActionBlockAsync(range, ips));
                    }
                    else if (ipOption.Where(x => char.IsLetter(x)).Count() > 0) //ips from file
                    {
                        totalIps = IOHelper.GetLinesCount(ipOption) * ports.Length;

                        var readedIps = IOHelper.ReadLineByLine(ipOption);

                        addIpAdresses = Task.Run(() => copyToActionBlockAsync(from ip in readedIps select IPAddress.Parse(ip), ips));
                    }
                    else //single ip
                    {
                        totalIps = ports.Length;

                        ips.Post(IPAddress.Parse(ipOption));
                    }
                }

                //Adding connection timeout
                double? connectionTimeout = o.ConnectionTimeout;

                if (connectionTimeout is not null)
                    timeout = connectionTimeout.Value;

            });

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
            currentRatio = calculateRatio(scannedIps, totalIps);

            Console.Write("\r{0:0.00}% - {1}/{2}", currentRatio, scannedIps, totalIps);

            await Task.Delay(100);
        }

        currentRatio = calculateRatio(scannedIps, totalIps);
        Console.Write("{0:0.00}% - {1}/{2}", currentRatio, scannedIps, totalIps);
        Console.Write("Waiting 10 sec for the results...");

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
            if (clients.Count > 0)
            {
                foreach (var client in clients)
                {
                    if (client.Disposed)
                        clients.Remove(client);
                    else if (DateTime.Now - client.initDateTime > timeToConnect && !client.isConnected)
                    {
                        client.Dispose();
                        clients.Remove(client);
                    }
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
        while ((totalIps - scannedIps) > 0)
        {
            foreach (ushort port in ports)
            {
                McClient client = new McClient(await ips.ReceiveAsync(), port, OnConnected);

                try
                {
                    client.BeginConnect();
                    clients.Add(client);
                    scannedIps++;
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

        McClient client = (McClient)result.AsyncState!;

        if (!client.isConnected)
        {
            client.Dispose();
            return;
        }

        string data = await client.GetServerInfo();

        if (data.StartsWith('{'))
        {
            try
            {
                string jsonData = StringPool.Shared.GetOrAdd(JsonHelper.ConvertToJsonString(data));

                ServerInfo serverInfo = new ServerInfo(jsonData, StringPool.Shared.GetOrAdd(client.IpEndPoint.Address.ToString()));
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
    static Thread updateDb = new(() =>
    {
        //Provides access to database
        DBController DB = new();

        int collectedInfosCount = 0;

        while (!endDBThread)
        {
            collectedInfosCount = serverInfos.Count;

            try
            {
                while (collectedInfosCount > 0)
                {
                    DB.AddOrUpdate(serverInfos.ReceiveAsync().Result).Wait();
                    collectedInfosCount--;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}: {1}; {2}", ex.Source, ex.Message, ex.InnerException?.Message ?? "");
            }

            Thread.Sleep(100);
        }
    });

    /// <summary>
    /// Asynchronously copy data from enumerable to actionBlock
    /// </summary>
    /// <typeparam name="T">Type of class to copy</typeparam>
    /// <param name="typeEnumerable">Copy from</param>
    /// <param name="typeActionBlock">Copy to</param>
    static async Task copyToActionBlockAsync<T>(IEnumerable<T> typeEnumerable, BufferBlock<T> typeActionBlock) where T : class
    {
        foreach (T item in typeEnumerable)
            await typeActionBlock.SendAsync(item);
    }

    /// <summary>
    /// Calculates percantage ratio of servers scanning progress
    /// </summary>
    /// <param name="currentInQueue">Current position in queue</param>
    /// <param name="length">Length of queue</param>
    /// <returns>Progress percentage rounded to 2 digits</returns>
    static double calculateRatio(double currentInQueue, double length) => currentInQueue / length * 100.0;
}