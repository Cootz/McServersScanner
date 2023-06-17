using BenchmarkDotNet.Attributes;
using System.Net;
using System.Threading.Tasks.Dataflow;
using McServersScanner.Core.Utils;

namespace McServersScanner.Benchmark.Utils;

[MemoryDiagnoser]
public class NetworkHelperBenchmark
{
    private string startIp = "50.0.0.0";
    private string endIp = "51.0.0.0";

    [Benchmark]
    public void GetIpRangeCountBenchmark()
    {
        NetworkHelper.GetIpRangeCount(startIp, endIp);
    }

    [Benchmark]
    public void FillIpRangeBenchmark()
    {
        BufferBlock<IPAddress> buffer = new();

        foreach (IPAddress? ip in NetworkHelper.FillIpRange(startIp, endIp))
        {
            buffer.Post(ip);
            buffer.Receive();
        }
    }
}