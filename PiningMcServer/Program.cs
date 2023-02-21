using CommandLine;
using McServersScanner.CLI;
using McServersScanner.IO.DB;
using McServersScanner.Network;
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
    private static BufferBlock<IPAddress> ips = new();

    /// <summary>
    /// Array of ports to scan
    /// </summary>
    private static ushort[] ports;

    /// <summary>
    /// Block of information about scanned servers
    /// </summary>
    private static BufferBlock<ServerInfo> serverInfos = new();

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    private static double timeout = 10;
    
    /// <summary>
    /// List of clients
    /// </summary>
    private static List<McClient> clients = new();

    private static async Task Main(string[] args)
    {
        //Parsing cmd params
        ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                //Adding ips
                List<string> ipRange = o.Range!.ToList();

                foreach (string ip in ipRange)
                {
                    if (ip.Contains('-')) //ip range
                    {
                        string[] splittedIps = ip.Split('-');

                        var range = NetworkHelper.FillIpRange(splittedIps[0], splittedIps[1]);
                        foreach (IPAddress ipAddr in range)
                            ips.Post(ipAddr);
                    }
                    else if (ip.Where(x => char.IsLetter(x)).Count() > 0) //ips from file
                    {
                        string[] readedIps = File.ReadAllLines(ip);

                        foreach (string ipAddr in readedIps)
                            if (!string.IsNullOrEmpty(ipAddr))
                                ips.Post(IPAddress.Parse(ipAddr));
                    }
                    else //single ip
                    {
                        ips.Post(IPAddress.Parse(ip));
                    }
                }

                //Adding ports
                List<string>? portList = o.Ports?.ToList();

                if (portList is null)
                    ports = new ushort[] { 25565 };
                else
                {
                    List<ushort> portUshot = new ();

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
            });

        int totalIps = ips.Count;
        ServicePointManager.DefaultConnectionLimit = 10000;
        double currentRatio;

        //Starting update db thread
        updateDb.Start();

        //Running workers
        Task writer = Task.Run(WriterAsync);
        Task reader = Task.Run(ReaderAsync);

        //Showing progress
        int currentCount;
        while (ips.Count > 0)
        {
            currentCount = ips.Count;
            currentRatio = calculateRatio(currentCount, totalIps);

            Console.Write("\r{0}% - {1}/{2}", currentRatio, totalIps - currentCount, totalIps);

            await Task.Delay(100);
        }

        currentCount = ips.Count;
        currentRatio = calculateRatio(currentCount, totalIps);
        Console.Write("{0}% - {1}/{2}", currentRatio, totalIps - currentCount, totalIps);
        Console.Write("Waiting 10 sec for the results...");

        await Task.WhenAll(writer, reader); //awaiting for results
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
            foreach (var client in clients)
            {
                if (DateTime.Now - client.initDateTime > timeToConnect && !client.isConnected)
                {
                    client.Dispose();
                    clients.Remove(client);
                }
                else if (client.Disposed)
                    clients.Remove(client);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        } while (clients.Count > 0);
    }

    /// <summary>
    /// Creates new <see cref="McClient"/>, starts and add it to <see cref="clients"/>
    /// </summary>
    /// <remarks>
    /// This task runs in different thread
    /// </remarks>
    public static async Task WriterAsync()
    {
        while (ips.Count > 0)
        {
            foreach (ushort port in ports)
            {
                McClient client = new McClient(await ips.ReceiveAsync(), port, OnConnected);

                try
                {
                    client.BeginConnect();
                    clients.Add(client);
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
            serverInfos.Post(new ServerInfo(data, client.IpEndPoint.Address.ToString()));
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
    /// Calculates percantage ratio of servers scanning progress
    /// </summary>
    /// <param name="currentInQueue">Current position in queue</param>
    /// <param name="length">Length of queue</param>
    /// <returns>Progress percentage rounded to 2 digits</returns>
    static double calculateRatio(double currentInQueue, double length) => Math.Round((length - currentInQueue) / length * 100.0, 2);
}