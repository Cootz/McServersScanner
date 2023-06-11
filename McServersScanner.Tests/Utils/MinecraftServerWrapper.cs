using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.Tests.Utils
{
    public class MinecraftServerWrapper
    {
        public string ServerPath { get; }
        public string ServerFile { get; }
        public string Version { get; }

        public Process ServerProc { get; private set; } = null!;

        public bool Running
        {
            get => !ServerProc?.HasExited ?? false;
        }

        public MinecraftServerWrapper(string serverFile, string version)
        {
            ServerPath = Path.GetDirectoryName(serverFile)!;
            ServerFile = Path.GetFileName(serverFile);
            Version = version;
        }

        public void Start()
        {
            ProcessStartInfo startInfo = new("java", "-Xmx1024M -Xms1024M -jar " + ServerFile + " nogui")
            {
                WorkingDirectory = ServerPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            ServerProc = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = false
            };

            ServerProc.Start();
        }

        public async Task WaitForLoad(TimeSpan? timeout = null)
        {
            const string stopFlag = "[Server thread/INFO]: Done";

            string? line;

            DateTime startTime = DateTime.Now;


            while (true)
            {
                line = await ServerProc.StandardOutput.ReadLineAsync();

                Debug.WriteLineIf(line is not null, line);

                if (line is not null) {
                    if (line.Contains(stopFlag))
                        return;
                }
                
                if (timeout is not null || DateTime.Now - startTime < timeout) return;
            }
        }

        public async Task StopAsync()
        {
            await ServerProc.StandardInput.WriteLineAsync("stop");
            await ServerProc.WaitForExitAsync();
        }
    }
}
