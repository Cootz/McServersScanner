using System.Net;
using System.Threading.Tasks.Dataflow;
using McServersScanner.Core;
using McServersScanner.Core.IO.DB;
using McServersScanner.Core.Utils;

namespace McServersScanner.Tests;

[TestFixture]
public class ScannerTest
{
    [Test]
    public void NormalScannerRun()
    {
        Scanner scanner = createBuilder().Build();

        Assert.DoesNotThrowAsync(async () => await scanner.Scan());

        Assert.That(File.Exists(DBController.DBPath));
    }

    [Test]
    public async Task ScannerRunWithForceStop()
    {
        Scanner scanner = createBuilder().Build();

        Task scan = scanner.Scan();

        await Task.Delay(90);

        Assert.DoesNotThrow(scanner.ForceStop);
    }

    private ScannerBuilder createBuilder()
    {
        ScannerBuilder scannerBuilder = new()
        {
            Ips = new BufferBlock<IPAddress>(new DataflowBlockOptions
            {
                BoundedCapacity = Scanner.DEFAULT_CONNECTION_LIMIT
            })
        };

        const string ipStartAddress = "127.0.0.1";
        const string ipEndAddress = "127.0.1.254";

        IEnumerable<IPAddress> iPs = NetworkHelper.FillIpRange(ipStartAddress, ipEndAddress);
        long ipCount = NetworkHelper.GetIpRangeCount(ipStartAddress, ipEndAddress);

        scannerBuilder.IpsCount = ipCount;
        scannerBuilder.AddIpAddresses =
            Task.Run(() => Scanner.CopyToActionBlockAsync(iPs, scannerBuilder.Ips));

        return scannerBuilder;
    }

    [TearDown]
    public void CleanUp()
    {
        DBController controller = new();

        controller.RealmQuerry((realm) => { realm.WriteAsync(realm.RemoveAll<ServerInfo>); });
    }
}