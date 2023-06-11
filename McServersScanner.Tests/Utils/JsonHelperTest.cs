using McServersScanner.Core.Utils;

namespace McServersScanner.Tests.Utils
{
    [TestFixture]
    public class JsonHelperTest
    {
        [Test]
        public void ConvertingToJsonStringTest()
        {
            string dirtyJson = "{ \"TestData\" : \"Some test data\"}fjis:f;";
            string expectedJson = "{ \"TestData\" : \"Some test data\"}";

            string cleanJson = JsonHelper.ConvertToJsonString(dirtyJson);

            Assert.That(cleanJson, Is.EqualTo(expectedJson));
        }
    }
}
