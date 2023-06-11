using System.Net;
using System.Threading.Tasks.Dataflow;
using McServersScanner.Core;
using McServersScanner.Core.IO.DB;
using McServersScanner.Core.Utils;

namespace McServersScanner.Tests
{
    [TestFixture]
    public class ScannerTest
    {
        [Test]
        public void NormalScannerRun()
        {
            ScannerConfiguration scannerConfiguration = new()
            {
                Ips = new(new DataflowBlockOptions
                {
                    BoundedCapacity = Scanner.ConnectionLimit
                })
            };

            string ipStartAddress = "127.0.0.1";
            string ipEndAddress = "127.0.1.254";

            IEnumerable<IPAddress> iPs = NetworkHelper.FillIpRange(ipStartAddress, ipEndAddress);
            long ipCount = NetworkHelper.GetIpRangeCount(ipStartAddress, ipEndAddress);

            scannerConfiguration.TotalIps = ipCount;
            scannerConfiguration.AddIpAddresses =
                Task.Run(() => Scanner.CopyToActionBlockAsync(iPs, scannerConfiguration.Ips));

            Scanner.ApplyConfiguration(scannerConfiguration);

            Assert.DoesNotThrowAsync(async () => await Scanner.Scan());

            Assert.That(File.Exists(DBController.DBPath));
        }

        [Test]
        public async Task ScannerRunWithForceStop()
        {
            ScannerConfiguration scannerConfiguration = new()
            {
                Ips = new(new DataflowBlockOptions
                {
                    BoundedCapacity = Scanner.ConnectionLimit
                })
            };

            const string ipStartAddress = "127.0.0.1";
            const string ipEndAddress = "127.0.1.254";

            IEnumerable<IPAddress> iPs = NetworkHelper.FillIpRange(ipStartAddress, ipEndAddress);
            long ipCount = NetworkHelper.GetIpRangeCount(ipStartAddress, ipEndAddress);

            scannerConfiguration.TotalIps = ipCount;
            scannerConfiguration.AddIpAddresses =
                Task.Run(() => Scanner.CopyToActionBlockAsync(iPs, scannerConfiguration.Ips));

            Scanner.ApplyConfiguration(scannerConfiguration);

            Task scan = Scanner.Scan();

            await Task.Delay(90);

            Assert.DoesNotThrow(Scanner.ForceStop);
        }

        [TearDown]
        public void CleanUp()
        {
            DBController controller = new();

            controller.RealmQuerry((realm) => { realm.WriteAsync(realm.RemoveAll<ServerInfo>); });

            Scanner.Reset();
        }
    }
}