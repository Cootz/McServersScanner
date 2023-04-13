using McServersScanner.Utils;

namespace McServersScanner.Tests.Utils
{
    [TestFixture]
    public class IOHelperTest
    {
        const int N = 10;

        [Test]
        public void TestGetLinesCount()
        {
            string filePath = generateTestFile();

            long linesCount = IOHelper.GetLinesCount(filePath);

            Assert.That(linesCount, Is.EqualTo(N));
        }

        [Test]
        public void TestReadFileLineByLine()
        {
            string filePath = generateTestFile();

            IEnumerable<string> lines = IOHelper.ReadLineByLine(filePath);
            IEnumerator<string> linesEnumerator = lines.GetEnumerator();

            for (int i = 0; i < N; i++)
            {
                linesEnumerator.MoveNext();
                Assert.That(linesEnumerator.Current, Is.EqualTo($"Line {i}"));
            }
        }

        private string generateTestFile()
        {
            string filePath = Path.GetTempFileName();

            using StreamWriter writer = new(filePath);

            for (int i = 0; i < N; i++)
                writer.WriteLine("Line {0}", i);

            return filePath;
        }
    }
}
