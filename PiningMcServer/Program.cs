using System;
using System.Net.Sockets;
using System.Text;

const int SEGMENT_BITS = 0x7F;
const int CONTINUE_BIT = 0x80;
const byte STRING_BREAK = 0xdd;
int delay = 10000;
int workingProcesses = 100;

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

    using (TcpClient client = new(ip, port))
    {
        client.SendTimeout = delay / 2;
        client.SendTimeout = delay / 2;

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
        List<byte> packet= new List<byte>();
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

Thread.Sleep(10000);

using (StreamWriter writer = new StreamWriter("serversOnline.txt"))
{
    foreach (var server in servers)
        writer.WriteLine(server);
}