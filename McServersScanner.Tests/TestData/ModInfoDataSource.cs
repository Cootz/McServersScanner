using McServersScanner.Core.IO.Database.Models;

namespace McServersScanner.Tests.TestData
{
    internal static class ModInfoDataSource
    {
        public static IEnumerable<TestCaseData> NegativeTestCases
        {
            get
            {
                yield return new TestCaseData(new ModInfo(), null);
                yield return new TestCaseData(new ModInfo("TestID", "TestVersion"), new ModInfo());
            }
        }
    }
}
