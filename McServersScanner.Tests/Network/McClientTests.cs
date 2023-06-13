using McServersScanner.Core.Network;

namespace McServersScanner.Tests.Network
{
    [TestFixture]
    public class McClientTests
    {
        [Test, Timeout(10000)]
        public async Task GetServerDataTest()
        {
            string json_server_info = string.Empty;

            McClient client = new("87.98.151.173", 25565,
                async result => { json_server_info = await ((McClient)result.AsyncState!).GetServerInfo(); },
                1000);

            client.BeginConnect();

            while (json_server_info == string.Empty)
            {
                await Task.Delay(50);
            }

            Console.WriteLine(json_server_info);
        }
    }
}
