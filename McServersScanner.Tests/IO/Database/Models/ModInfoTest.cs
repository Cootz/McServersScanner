using McServersScanner.Core.IO.Database.Models;
using McServersScanner.Tests.TestData;

namespace McServersScanner.Tests.IO.Database.Models
{
    [TestFixture]
    public class ModInfoTest
    {
        [Test]
        public void Equality_WithTwoEqualModInfos_ReturnsTrue()
        {
            ModInfo first = new()
            {
                ModId = "TestID",
                Version = "TestVersion"
            };
            ModInfo second = new()
            {
                ModId = "TestID",
                Version = "TestVersion"
            };

            Assert.That(first, Is.EqualTo(second));
        }

        [TestCaseSource(typeof(ModInfoDataSource), nameof(ModInfoDataSource.NegativeTestCases))]
        public void Equality_WithTwoDifferentModInfos_ReturnsFalse(ModInfo first, ModInfo second)
        {
            Assert.That(first, Is.Not.EqualTo(second));
        }
    }
}
