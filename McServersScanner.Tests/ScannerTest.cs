using McServersScanner.IO.DB;
using McServersScanner.Utils;
using System.Net;
using System.Threading.Tasks.Dataflow;

namespace McServersScanner.Tests
{
    [TestFixture]
    public class ScannerTest
    {
        [Test]
        public void NormalScannerRun()
        {
            ScannerConfiguration scannerConfiguration = new();

            scannerConfiguration.ips = new(new DataflowBlockOptions()
            {
                BoundedCapacity = Scanner.ConnectionLimit
            });

            string ipStartAddress = "127.0.0.1";
            string ipEndAddress = "127.0.1.254";

            IEnumerable<IPAddress> iPs = NetworkHelper.FillIpRange(ipStartAddress, ipEndAddress);
            long ipCount = NetworkHelper.GetIpRangeCount(ipStartAddress, ipEndAddress);

            scannerConfiguration.totalIps = ipCount;
            scannerConfiguration.addIpAdresses = Task.Run(() => Scanner.copyToActionBlockAsync(iPs, scannerConfiguration.ips));

            Scanner.ApplyConfiguration(scannerConfiguration);

            Assert.DoesNotThrowAsync(async () => await Scanner.Scan());

            Assert.True(File.Exists(DBController.DBPath));

        }

        [Test]
        public async Task ScannerRunWithForceStop()
        {
            ScannerConfiguration scannerConfiguration = new();

            scannerConfiguration.ips = new(new DataflowBlockOptions()
            {
                BoundedCapacity = Scanner.ConnectionLimit
            });

            string ipStartAddress = "127.0.0.1";
            string ipEndAddress = "127.0.1.254";

            IEnumerable<IPAddress> iPs = NetworkHelper.FillIpRange(ipStartAddress, ipEndAddress);
            long ipCount = NetworkHelper.GetIpRangeCount(ipStartAddress, ipEndAddress);

            scannerConfiguration.totalIps = ipCount;
            scannerConfiguration.addIpAdresses = Task.Run(() => Scanner.copyToActionBlockAsync(iPs, scannerConfiguration.ips));

            Scanner.ApplyConfiguration(scannerConfiguration);

            var scan = Scanner.Scan();

            await Task.Delay(50);

            Assert.DoesNotThrow(() => Scanner.ForceStop());

            Assert.True(scan.IsCompleted);
        }

        [TearDown]
        public void CleanUp()
        {
            DBController controller = new DBController();

            controller.RealmQuerry((realm) =>
            {
                realm.WriteAsync(() => realm.RemoveAll<ServerInfo>());
            });

            Scanner.Reset();
        }
    }
}
