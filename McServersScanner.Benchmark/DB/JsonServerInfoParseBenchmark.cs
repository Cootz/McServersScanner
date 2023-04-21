using BenchmarkDotNet.Attributes;
using McServersScanner.IO.DB;
using Microsoft.Diagnostics.Tracing.Parsers.AspNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.Benchmark.DB
{
    [MemoryDiagnoser]
    public class JsonServerInfoParseBenchmark
    {
        public const string json = "{\"version\":{\"name\":\"1.19.4\",\"protocol\":762},\"description\":{\"text\":\"1.19.4 on angel.lanri.space\"},\"players\":{\"max\":20,\"online\":1}}";
        public const string ip = "10.0.0.27";

        [Benchmark]
        public void DeserializeToServerInfo()
        {
            _ = new ServerInfo(json, ip);
        }
    }
}
