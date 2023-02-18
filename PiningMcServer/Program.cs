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
    /// List of Ips to scan
    /// </summary>
    private static BufferBlock<IPAddress> ips = new();
    
    /// <summary>
    /// 
    /// </summary>
    private static BufferBlock<ServerInfo> serverInfos = new();

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    private static double timeout = 10;
    private static DBController DB = new();
    private static List<McClient> clients = new();

    private static async Task Main(string[] args)
    {
        //Init
        Task DBinit = DB.Initialize();

        //Parsing cmd params
        ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                //Adding ips
                List<string> ipRange = o.Range?.ToList() ?? new List<string>();

                foreach (string ip in ipRange)
                {
                    if (ip.Contains('-')) //ip range
                    {
                        string[] splittedIps = ip.Split('-');

                        var range = IpAddressGeneratior.FillRange(splittedIps[0], splittedIps[1]);
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
            });

        int totalIps = ips.Count;
        ServicePointManager.DefaultConnectionLimit = 10000;
        List<Task> tasks = new();
        double currentRatio;

        //Awaiting db initialization
        await DBinit;

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
        endDBThread = true;//ending db thread
        updateDb.Join();

        //Saving db
        DB.SaveChanges();
        DB.Dispose();
    }

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

    public static async Task WriterAsync()
    {
        while (ips.Count > 0)
        {
            McClient client = new McClient(await ips.ReceiveAsync(), 25565, OnConnected);
            try
            {
                client.BeginConnect();
                clients.Add(client);
            }
            catch { }
        }
    }

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

    static Thread updateDb = new(() =>
    {
        int collectedInfosCount = 0;

        while (!endDBThread)
        {
            collectedInfosCount = serverInfos.Count;
            bool isExecuted = collectedInfosCount > 0;

            try
            {
                while (collectedInfosCount > 0)
                {
                    DB.AddOrUpdate(serverInfos.ReceiveAsync().Result).Wait();
                    collectedInfosCount--;
                }

                if (isExecuted)
                    DB.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}: {1}; {2}", ex.Source, ex.Message, ex.InnerException?.Message ?? "");
            }

            Thread.Sleep(100);
        }
    });

    static double calculateRatio(double currentInQueue, double length) => Math.Round((length - currentInQueue) / length * 100.0, 2);
}