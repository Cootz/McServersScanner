using BenchmarkDotNet.Attributes;
using System.Net;
using System.Threading.Tasks.Dataflow;
using McServersScanner.Core.Utils;

namespace McServersScanner.Benchmark.Utils;

[MemoryDiagnoser]
public class NetworkHelperBenchmark
{
    private string startIp = "50.0.0.0";
    private string endIP = "51.0.0.0";

    [Benchmark]
    public void GetIpRangeCountBenchmark()
    {
        NetworkHelper.GetIpRangeCount(startIp, endIP);
    }

    [Benchmark]
    public void FillIpRangeBenchmark()
    {
        BufferBlock<IPAddress> buffer = new();

        foreach (IPAddress? ip in NetworkHelper.FillIpRange(startIp, endIP))
        {
            buffer.Post(ip);
            buffer.Receive();
        }
    }
}