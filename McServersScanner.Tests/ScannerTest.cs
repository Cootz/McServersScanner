using System.Net;
using System.Threading.Tasks.Dataflow;
using McServersScanner.Core;
using McServersScanner.Core.IO.Database;
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

        Assert.That(File.Exists(DatabaseController.DatabasePath));

    }

    private ScannerBuilder createBuilder()
    {
        ScannerBuilder builder = new()
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

        builder.IpsCount = ipCount;
        builder.AddIpAddresses =
            Task.Run(() => Scanner.CopyToActionBlockAsync(iPs, builder.Ips));

        return builder;
    }

    [TearDown]
    public void CleanUp()
    {
        DatabaseController controller = new();

        controller.RealmQuery((realm) => { realm.WriteAsync(realm.RemoveAll<ServerInfo>); });
    }
}