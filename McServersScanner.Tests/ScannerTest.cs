using McServersScanner.IO.DB;
using McServersScanner.Utils;
using System.Net;
using System.Threading.Tasks.Dataflow;
using McServersScanner.Tests.Utils;
using Microsoft.VisualBasic.FileIO;
using Realms;

namespace McServersScanner.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class ScannerTest
    {
        [Test]
        public void NormalScannerRun()
        {
            ScannerConfiguration scannerConfiguration = new()
            {
                ips = new(new DataflowBlockOptions()
                {
                    BoundedCapacity = Scanner.ConnectionLimit
                })
            };

            string ipStartAddress = "127.0.0.1";
            string ipEndAddress = "127.0.1.254";

            IEnumerable<IPAddress> iPs = NetworkHelper.FillIpRange(ipStartAddress, ipEndAddress);
            long ipCount = NetworkHelper.GetIpRangeCount(ipStartAddress, ipEndAddress);

            scannerConfiguration.totalIps = ipCount;
            scannerConfiguration.addIpAdresses = Task.Run(() => Scanner.CopyToActionBlockAsync(iPs, scannerConfiguration.ips));

            Scanner.ApplyConfiguration(scannerConfiguration);

            Assert.DoesNotThrowAsync(async () => await Scanner.Scan());

            Assert.That(File.Exists(DBController.DBPath));
        }

        [Test]
        public async Task ScannerRunWithForceStop()
        {
            ScannerConfiguration scannerConfiguration = new()
            {
                ips = new(new DataflowBlockOptions()
                {
                    BoundedCapacity = Scanner.ConnectionLimit
                })
            };

            string ipStartAddress = "127.0.0.1";
            string ipEndAddress = "127.0.1.254";

            IEnumerable<IPAddress> iPs = NetworkHelper.FillIpRange(ipStartAddress, ipEndAddress);
            long ipCount = NetworkHelper.GetIpRangeCount(ipStartAddress, ipEndAddress);

            scannerConfiguration.totalIps = ipCount;
            scannerConfiguration.addIpAdresses = Task.Run(() => Scanner.CopyToActionBlockAsync(iPs, scannerConfiguration.ips));

            Scanner.ApplyConfiguration(scannerConfiguration);

            var scan = Scanner.Scan();

            await Task.Delay(90);

            Assert.DoesNotThrow(Scanner.ForceStop);

            Assert.That(scan.IsCompleted);
        }

        [Test]
        public async Task ReceiveDataFromServerTest()
        {
            MinecraftServerWrapper server = new("Data/Mc1.19.2/server.jar", "1.19.2");

            server.Start();

            Task load = server.WaitForLoad();

            ScannerConfiguration scannerConfiguration = new()
            {
                ips = new BufferBlock<IPAddress>(new DataflowBlockOptions
                {
                    BoundedCapacity = Scanner.ConnectionLimit
                }),
                totalIps = 1
            };

            scannerConfiguration.ips.Post(IPAddress.Parse("172.0.0.1"));

            Scanner.ApplyConfiguration(scannerConfiguration);

            Assert.That(server.Running);

            await load;

            Assert.DoesNotThrowAsync(async () => await Scanner.Scan());

            Task stop = server.StopAsync();

            DBController controller = new();

            ServerInfo serverInfo = null!;

            controller.RealmQuerry(realm =>
            {
                serverInfo = realm.All<ServerInfo>().First();
            });

            await stop;

            Assert.Multiple(() =>
            {
                Assert.That(serverInfo.Online, Is.EqualTo(0));
                Assert.That(serverInfo.Description, Is.EqualTo("Test minecraft server"));
                Assert.That(serverInfo.Version.Name, Is.EqualTo("1.19.2"));
            });
        }

        [TearDown]
        public void CleanUp()
        {
            DBController controller = new();

            controller.RealmQuerry((realm) =>
            {
                realm.WriteAsync(realm.RemoveAll<ServerInfo>);
            });

            Scanner.Reset();
        }
    }
}
