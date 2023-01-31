using CommandLine;
using McServersScanner.CLI;
using McServersScanner.IO.DB;
using McServersScanner.Network;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;

internal class Program
{
    private static bool endDBThread = false;
    private static BufferBlock<IPAddress> ips = new();
    private static BufferBlock<ServerInfo> serverInfos = new();
    private static int timeout = 500;
    private static int workingProcesses = 1000;
    private static DBController DB = new();

    private static async Task Main(string[] args)
    {
        //Init
        Task DBinit = DB.Initialize();

        //Parsing cmd params
        ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                //Adding ips
                List<string> ipRange = o.Range.ToList();

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
                        string[] readedIps = File.ReadAllLinesAsync(ip).Result;

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

        //HelpText.AutoBuild<Options>(result);

        int totalIps = ips.Count;

        ServicePointManager.DefaultConnectionLimit = workingProcesses;
        List<Task> tasks = new();
        double currentRatio;

        //Awaiting db initialization
        await DBinit;
        
        //Starting update db thread
        updateDb.Start();

        //Running workers
        for (int i = 0; i < workingProcesses; i++)
        {
            tasks.Add(Task.Run(WorkingThread));
        }

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
        Console.Write("\r{0}% - {1}/{2}", currentRatio, totalIps - currentCount, totalIps);

        await Task.Delay(1000); //awaiting for results
        endDBThread = true;//ending db thread
        updateDb.Join();

        //Saving db
        DB.SaveChanges();
        DB.Dispose();
    }

    private static async Task WorkingThread()
    {
        while (ips.Count > 0)
        {
            IPAddress ip = await ips.ReceiveAsync();

            try
            {
                string data = string.Empty;

                using (McClient mcClient = new McClient(ip, 25565))
                    data = await mcClient.GetServerInfo(timeout);

                if (data.FirstOrDefault() == '{')
                {
                    //Console.WriteLine(ip);
                    serverInfos.Post(new ServerInfo(data, ip.ToString()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0}: {1}; {2} occured at ip {3}", ex.Source, ex.Message, ex.InnerException?.Message ?? "", ip);
            }
        }
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