using McServersScanner.IO.DB;
using McServersScanner.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks.Dataflow;


bool endThread = false;
BufferBlock<IPAddress> ips = new();
BufferBlock<ServerInfo> serverInfos = new();
int timeout = 500;
int workingProcesses = 1000;

double calculateRatio(double currentInQueue, double length) => Math.Round(((length - currentInQueue)/ length) * 100.0, 2);

DBController DB = new DBController();

async Task WorkingThread()
{
    while (ips.Count > 0)
    {
        IPAddress ip = await ips.ReceiveAsync();

        try
        {
            string data = String.Empty;

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
};

await DB.Initialize();

string[] readedIps = File.ReadAllLinesAsync("ips.txt").Result;
int totalIps = readedIps.Length;

foreach (var ip in readedIps)
    if (!String.IsNullOrEmpty(ip))
        ips.Post(IPAddress.Parse(ip));


ServicePointManager.DefaultConnectionLimit = workingProcesses;
List<Task> tasks= new ();
double prevRatio = 0;
double currentRatio;

Thread updateDb = new(() =>
{
    int collectedInfosCount = 0;

    while (!endThread)
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
updateDb.Start();

for (int i = 0; i < workingProcesses; i++)
{
    tasks.Add(Task.Run(WorkingThread));
}

int currentCount;
while (ips.Count > 0)
{
    currentCount = ips.Count;
    currentRatio = calculateRatio(currentCount, totalIps);

    Console.Write("\r{0}% - {1}/{2}", currentRatio, (totalIps - currentCount), totalIps);

    await Task.Delay(100);
}

currentCount = ips.Count;
currentRatio = calculateRatio(currentCount, totalIps);
Console.Write("\r{0}% - {1}/{2}", currentRatio, (totalIps - currentCount), totalIps);

await Task.Delay(10000);
endThread = true;
updateDb.Join();

DB.SaveChanges();
DB.Dispose();