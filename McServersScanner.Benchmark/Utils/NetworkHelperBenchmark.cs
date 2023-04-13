using BenchmarkDotNet.Attributes;
using McServersScanner.Utils;
using System.Net;
using System.Threading.Tasks.Dataflow;

namespace McServersScanner.Benchmark.Utils
{
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
            BufferBlock<IPAddress> buffer = new BufferBlock<IPAddress>();

            foreach (var ip in NetworkHelper.FillIpRange(startIp, endIP))
            {
                buffer.Post(ip);
                buffer.Receive();
            }
        }
    }
}
