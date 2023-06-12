using McServersScanner.Core.IO.Database;
using McServersScanner.Tests.TestData;

namespace McServersScanner.Tests.IO.Database;

[TestFixture]
public class DatabaseControllerTest
{
    private readonly IDatabaseController databaseController;

    public DatabaseControllerTest() => databaseController = new DatabaseController();

    [Test]
    public void AddServerInfoTest()
    {
        ServerInfo serverInfo = ServerInfoDataSource.TestServerInfo;

        databaseController.Add(serverInfo);

        Assert.That(databaseController.Select<ServerInfo>().First(), Is.EqualTo(serverInfo));
    }

    [TearDown]
    public void TearDown()
    {
        databaseController.RealmQuery(r => r.Write(r.RemoveAll<ServerInfo>));
    }
}