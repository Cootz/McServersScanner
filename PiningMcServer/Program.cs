using McServersScanner.DB;
using McServersScanner.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

const int SEGMENT_BITS = 0x7F;
const int CONTINUE_BIT = 0x80;
const byte STRING_BREAK = 0xdd;
Queue<string> ips = new ();
Queue<ServerInfo> serverInfos = new ();
int timeout = 1000;
int workingProcesses = 1000;

List<byte> writeVarInt(int value)
{
    List<byte> buffer = new();

    while ((value & CONTINUE_BIT) != 0)
    {
        buffer.Add((byte)(value & SEGMENT_BITS | CONTINUE_BIT));
        value = (int)((uint)value) >> 7;
    }

    buffer.Add((byte)value);
    return buffer;
}

List<byte> writeString(string value)
{
    byte[] valAsBytes = Encoding.UTF8.GetBytes(value);
    List<byte> buffer = new();

    buffer.AddRange(writeVarInt(valAsBytes.Length));
    buffer.AddRange(valAsBytes);

    return buffer;
}

async Task<string> GetServerData(string ip)
{
    ushort port = 25565;
    int protocolVersion = 761;
    StringBuilder response = new StringBuilder();

    try 
    {
        using (TcpClient client = new())
        {
            try
            {
                var result = client.BeginConnect(ip, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(timeout, true); //Waiting connection for a timeout

                if (!success)//Stop exec if not connected
                {
                    client.Close();
                    return String.Empty;
                }

                client.ReceiveTimeout = timeout / 2;
                client.SendTimeout = timeout / 2;

                //Creating handshake data
                int nextState = 1;
                List<byte> data = new List<byte>
            {
                0x0//handshake packet id
            };
                data.AddRange(writeVarInt(protocolVersion));//Version
                data.AddRange(writeString(ip));//Ip
                data.AddRange(BitConverter.GetBytes(port));//Port
                data.AddRange(writeVarInt(nextState));//Next state

                //Handshake packet
                List<byte> packet = new List<byte>();
                packet.AddRange(writeVarInt(data.Count));
                packet.AddRange(data);

                //Send handshake
                await client.GetStream().WriteAsync(packet.ToArray(), 0, packet.Count);

                //Send ping req
                byte[] pingData = { 1, 0 };
                await client.GetStream().WriteAsync(pingData, 0, 2);

                byte[] buffer = new byte[1024];

                using (NetworkStream ns = client.GetStream())
                {
                    do
                    {
                        await ns.ReadAsync(buffer, 0, buffer.Length);
                        response.Append(Encoding.UTF8.GetString(buffer));
                    } while (client.Available > 0);
                }
            }
            catch (Exception ex)
            {
                client.Close();
                return String.Empty;
            }
        }
    }
    catch(Exception ex)
    {
        return String.Empty;
    }

    //Preventing "System.Private.CoreLib:Index and count must refer to a location within the string. (Parameter 'count')" exception
    string ret = response.ToString();

    if (ret.Length > 5)
        return ret.Remove(0, 5);
    else
        return String.Empty;

}

double calculateRatio(double currentInQueue, double length) => Math.Round(((length - currentInQueue)/ length) * 100.0, 2);

DBController DB = new DBController();

async Task WorkingThread()
{
    while (ips.Count > 0)
    {
        string ip = ips.Dequeue();

        try
        {
            string data = String.Empty;

            data = await GetServerData(ip);

            if (data.FirstOrDefault() == '{')
            {
                //Console.WriteLine(ip);
                serverInfos.Enqueue(new ServerInfo(data, ip));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("{0}: {1}; {2}", ex.Source, ex.Message, ex.InnerException.Message);
        }
    }
};

await DB.Initialize();

string[] readedIps = File.ReadAllLinesAsync("ips.txt").Result;
int totalIps = readedIps.Length;
foreach (var ip in readedIps)
    ips.Enqueue(ip);


ServicePointManager.DefaultConnectionLimit = workingProcesses;
List<Task> tasks= new ();
double prevRatio = 0;
double currentRatio;

Thread updateDb = new(() =>
{
    while (true)
    {
        bool isExecuted = serverInfos.Count > 0;

        try
        {
            while (serverInfos.Count > 0)
                DB.AddOrUpdate(serverInfos.Dequeue()).Wait();
            if (isExecuted)
                DB.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine("{0}: {1}; {2}", ex.Source, ex.Message, ex.InnerException.Message);
        }

        Thread.Sleep(timeout);
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

foreach (var task in tasks)
    await task;