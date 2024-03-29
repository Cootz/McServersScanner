﻿using System.Diagnostics;
using System.Text;
using McServersScanner.Core.IO;

namespace McServersScanner.Tests.IO
{
    [TestFixture]
    public class ThrottleStreamTest
    {
        [Test]
        public async Task IOThrottlingTest()
        {
            ThrottleManager manager = new(128);

            SharedThrottledStream throttledStream = new(new MemoryStream(), manager);

            const string data = "Some pretty long test that you need to send over the web.";

            byte[] buffer = Encoding.Unicode.GetBytes(data);

            Stopwatch sw = Stopwatch.StartNew();

            await throttledStream.WriteAsync(buffer);
            await throttledStream.WriteAsync(buffer);

            sw.Stop();

            Assert.That(sw.Elapsed.TotalSeconds, Is.EqualTo(1d).Within(0.12d));
        }

        [Test]
        public async Task MultipleStreamsThrottlingTest()
        {
            ThrottleManager manager = new(1024);

            SharedThrottledStream[] throttledStreams =
            {
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager),
                new(new MemoryStream(), manager)
            };

            Random rnd = new();

            SemaphoreSlim semaphoreSlim = new(0);

            Task gunShot = semaphoreSlim.WaitAsync();

            byte[] buffer = new byte[128];

            rnd.NextBytes(buffer);

            List<Task> tasks = new();

            for (int i = 0; i < 2; i++)
            {
                tasks.AddRange(throttledStreams.Select(
                    throttledStream => Task.Run(async () =>
                    {
                        await gunShot;
                        await throttledStream.WriteAsync(buffer);
                    }))
                );
            }

            Stopwatch sw = Stopwatch.StartNew();

            //Start all streams at the same time
            semaphoreSlim.Release();

            await Task.WhenAll(tasks);

            sw.Stop();

            Assert.That(sw.Elapsed.TotalSeconds, Is.EqualTo(3d).Within(0.1d));
        }
    }
}
