using McServersScanner.DB;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

const int SEGMENT_BITS = 0x7F;
const int CONTINUE_BIT = 0x80;
const byte STRING_BREAK = 0xdd;
int timeout = 1000;
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

    using (TcpClient client = new())
    {
        var result = client.BeginConnect(ip, port, null, null);
        var success = result.AsyncWaitHandle.WaitOne(timeout); //Waiting connection for a timeout

        if (!success)//Stop exec if not connected
        { 
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

    //Preventing "System.Private.CoreLib:Index and count must refer to a location within the string. (Parameter 'count')" exception
    string ret = response.ToString();

    if (ret.Length > 5)
        return ret.Remove(0, 5);
    else 
        return String.Empty;
}

double calculateRatio(double current, double length) => Math.Round((current / length) * 100.0, 2);

DBController DB = new DBController();

async Task Action(string ip)
{
    string data = String.Empty;

    data = await GetServerData(ip);

    if (data == String.Empty)
    {
        return;
    }

    if (data.FirstOrDefault() == '{')
    {
        //Console.WriteLine(ip);
        await DB.Add(new ServerInfo(data, ip));
    }
};

await DB.Initialize();
string[] ips = File.ReadAllLinesAsync("ips.txt").Result;

SemaphoreSlim maxThread = new SemaphoreSlim(workingProcesses);
Task[] tasks= new Task[ips.Length];
double prevRatio = 0;
double currentRatio;

for (int i = 0; i < ips.Length; i++)
{
    currentRatio = calculateRatio(i, ips.Length);
    
    if (currentRatio > prevRatio)
    {
        Console.Write("\r{0}% - {1}/{2}", currentRatio, i, ips.Length);
        prevRatio = currentRatio;
    }

    maxThread.Wait();
    tasks[i] = Task.Run(
            () => Action(ips[i])
            .ContinueWith((task) => maxThread.Release())
        );
}

Task.WhenAll( tasks ).Wait();