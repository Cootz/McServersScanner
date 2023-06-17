using McServersScanner.Core.Utils;

namespace McServersScanner.Tests.Utils;

[TestFixture]
public class IOHelperTest
{
    private const int n = 10;

    [Test]
    public void TestGetLinesCount()
    {
        string filePath = generateTestFile();

        long linesCount = IOHelper.GetLinesCount(filePath);

        Assert.That(linesCount, Is.EqualTo(n));
    }

    [Test]
    public void TestReadFileLineByLine()
    {
        string filePath = generateTestFile();

        IEnumerable<string> lines = IOHelper.ReadLineByLine(filePath);
        IEnumerator<string> linesEnumerator = lines.GetEnumerator();

        for (int i = 0; i < n; i++)
        {
            linesEnumerator.MoveNext();
            Assert.That(linesEnumerator.Current, Is.EqualTo($"Line {i}"));
        }
    }

    private static string generateTestFile()
    {
        string filePath = Path.GetTempFileName();

        using StreamWriter writer = new(filePath);

        for (int i = 0; i < n; i++)
            writer.WriteLine("Line {0}", i);

        return filePath;
    }
}