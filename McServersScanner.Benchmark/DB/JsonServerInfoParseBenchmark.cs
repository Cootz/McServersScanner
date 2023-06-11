using BenchmarkDotNet.Attributes;
using McServersScanner.Core.IO.DB;

namespace McServersScanner.Benchmark.DB;

[MemoryDiagnoser]
public class JsonServerInfoParseBenchmark
{
    public const string JSON =
        "{\"version\":{\"name\":\"1.19.4\",\"protocol\":762},\"description\":{\"text\":\"1.19.4 on angel.lanri.space\"},\"players\":{\"max\":20,\"online\":1}}";

    public const string IP = "10.0.0.27";

    [Benchmark]
    public void DeserializeToServerInfo()
    {
        _ = new ServerInfo(JSON, IP);
    }
}