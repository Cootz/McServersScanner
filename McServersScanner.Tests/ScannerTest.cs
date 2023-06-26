using System.Net;
using System.Threading.Tasks.Dataflow;
using McServersScanner.Core;
using McServersScanner.Core.IO.Database;
using McServersScanner.Core.IO.Database.Models;
using McServersScanner.Core.Utils;
using NSubstitute;

namespace McServersScanner.Tests;

[TestFixture]
public class ScannerTest
{
    private static Stream outputStream = null!;

    [Test]
    public void NormalScannerRun()
    {
        Scanner scanner = createBuilder().Build();

        Assert.DoesNotThrowAsync(async () => await scanner.Scan());

        Assert.That(File.Exists(DatabaseController.DatabasePath));
    }

    [Test]
    public async Task ScannerRunWithForceStop()
    {
        Scanner scanner = createBuilder().Build();

        Task scan = scanner.Scan();

        await Task.Delay(90);

        Assert.DoesNotThrow(scanner.ForceStop);
    }

    private static ScannerBuilder createBuilder()
    {
        outputStream = Substitute.For<Stream>();

        outputStream.CanWrite.Returns(true);

        ScannerBuilder builder = new()
        {
            Ips = new BufferBlock<IPAddress>(new DataflowBlockOptions
            {
                BoundedCapacity = ScannerBuilder.DEFAULT_CONNECTION_LIMIT
            }),
            Timeout = 0.3d,
            ConnectionLimit = 100,
            OutputStream = new StreamWriter(outputStream)
        };

        const string ipStartAddress = "127.0.0.1";
        const string ipEndAddress = "127.0.1.254";

        IEnumerable<IPAddress> iPs = NetworkHelper.FillIpRange(ipStartAddress, ipEndAddress);
        long ipCount = NetworkHelper.GetIpRangeCount(ipStartAddress, ipEndAddress);

        builder.IpsCount = ipCount;
        builder.AddIpAddresses =
            Task.Run(() => Scanner.CopyToBufferBlockAsync(iPs, builder.Ips));

        return builder;
    }

    [TearDown]
    public void CleanUp()
    {
        DatabaseController controller = new();

        controller.RealmQuery((realm) => { realm.WriteAsync(realm.RemoveAll<ServerInfo>); });
    }
}