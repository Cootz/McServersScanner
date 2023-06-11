using McServersScanner.Core.Utils;

namespace McServersScanner.Tests.Utils
{
    [TestFixture]
    public class ParsingHelperTest
    {
        [Test]
        [TestCase("10", ExpectedResult = 10)]
        [TestCase("1K", ExpectedResult = 1024)]
        [TestCase("2K", ExpectedResult = 2048)]
        [TestCase("1M", ExpectedResult = 1_048_576)]
        [TestCase("1M", ExpectedResult = 1_048_576)]
        [TestCase("5M", ExpectedResult = 5_242_880)]
        public int TestConvertBandwidthLimitToString(string bandwidthLimit) =>
            ParsingHelper.ConvertToNumberOfBytes(bandwidthLimit);
    }
}
