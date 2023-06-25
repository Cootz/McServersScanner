using McServersScanner.Core.IO.Database;
using McServersScanner.Core.IO.Database.Models;
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
        ServerInfo serverInfo = new(ServerInfoDataSource.GetJsonServerInfo(), ServerInfoDataSource.SERVER_INFO_IP);

        databaseController.Add(serverInfo);

        Assert.That(databaseController.Select<ServerInfo>().First(), Is.EqualTo(serverInfo));
    }

    [TearDown]
    public void TearDown()
    {
        databaseController.RealmQuery(r => r.Write(r.RemoveAll<ServerInfo>));
    }
}