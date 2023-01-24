// See https://aka.ms/new-console-template for more information
using System.Data;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks.Dataflow;

int SEGMENT_BITS = 0x7F;
int CONTINUE_BIT = 0x80;
int delay = 1000;
int workingProcesses = 100;

List<byte> writeVarInt(int value)
{
    List<byte> buffer = new();

    while (true)
    {
        if ((value & ~SEGMENT_BITS) == 0)
        {
            buffer.Add((byte)value);
            return buffer;
        }

        buffer.Add((byte)((value & SEGMENT_BITS) | CONTINUE_BIT));

        // Note: >>> means that the sign bit is shifted with the rest of the number rather than being left alone
        value >>= 7;
    }
}

async Task<string> GetServerData(string ip)
{
    ushort port = 25565;
    int protocolVersion = 335;
    StringBuilder response = new StringBuilder();

    using (TcpClient client = new(ip, port))
    {
        client.SendTimeout = delay / 2;
        client.SendTimeout = delay / 2;

        //Creating packet
        Byte nextState = 1;
        List<byte> data = new List<byte>();
        data.AddRange(new byte[] { 0x15, 0x00 });
        data.AddRange(writeVarInt(protocolVersion));//Version
        data.Add(0x0e);
        data.AddRange(Encoding.UTF8.GetBytes(ip).SkipLast(1));//Ip
        data.AddRange(BitConverter.GetBytes(port));//Port
        data.Add(0xdd);
        data.Add(nextState);//Next state

        await client.GetStream().WriteAsync(data.ToArray(), 0, data.Count);

        byte[] pingData = { 1, 0 };
        await client.GetStream().WriteAsync(pingData, 0, 2);

        byte[] buffer = new byte[1024];

        using (NetworkStream ns = client.GetStream())
        {
            do
            {
                try
                {            
                    await ns.ReadAsync(buffer, 0, buffer.Length);
                    response.Append(Encoding.UTF8.GetString(buffer));
                }
                catch { }

            } while (client.Available > 0);
        }
    }

    return response.ToString().Remove(0, 5);
}

List<string> servers = new List<string>();

async Task Action(string ip)
{
    string data = await GetServerData(ip);
    if (data.FirstOrDefault() == '{')
    {
        string ret = String.Format("{0} - ip: {1}", data, ip);
        Console.WriteLine(ret);
        servers.Add(ret);
    }
};

string[] ips = File.ReadAllLinesAsync("ips.txt").Result;

SemaphoreSlim maxThread = new SemaphoreSlim(workingProcesses);

foreach (string ip in ips)
{
    maxThread.Wait();
    Task.Factory.StartNew(
        () => Action(ip),
        TaskCreationOptions.LongRunning)
        .ContinueWith((task) => maxThread.Release());
}

using (StreamWriter writer = new StreamWriter("serversOnline.txt"))
{
    foreach (var server in servers)
        writer.WriteLine(server);
}
    




