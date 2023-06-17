using System.Net;
using McServersScanner.Core.Utils;

namespace McServersScanner.Tests.Utils
{
    [TestFixture]
    public class NetworkHelperTest
    {
        private const string first_ip = "192.168.0.0";
        private const string last_ip = "192.168.0.5";

        [Test]
        public void FillRangeOfIpsTest()
        {
            IReadOnlyCollection<IPAddress> expectedIps = new[]
            {
                IPAddress.Parse(first_ip),
                IPAddress.Parse("192.168.0.1"),
                IPAddress.Parse("192.168.0.2"),
                IPAddress.Parse("192.168.0.3"),
                IPAddress.Parse("192.168.0.4"),
                IPAddress.Parse(last_ip)
            };

            IEnumerable<IPAddress> ips = NetworkHelper.FillIpRange(first_ip, last_ip);

            Assert.That(ips, Is.EqualTo(expectedIps));
        }

        [Test]
        public void GetIpRangeCountTest()
        {
            const long expectedCount = 6;

            long count = NetworkHelper.GetIpRangeCount(first_ip, last_ip);

            Assert.That(count, Is.EqualTo(expectedCount));
        }

        [Test]
        public void FillRangeOfPortsTest()
        {
            const ushort firstPort = 25565;
            const ushort lastPort = 25567;

            IReadOnlyCollection<ushort> expectedPorts = new ushort[]
            {
                firstPort,
                25566,
                lastPort
            };

            IEnumerable<ushort> ports = NetworkHelper.FillPortRange(firstPort, lastPort);

            Assert.That(ports, Is.EqualTo(expectedPorts));
        }
    }
}
