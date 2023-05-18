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

        public Process ServerProc { get; private set; }

        public bool Running
        {
            get => !ServerProc?.HasExited ?? false;
        }

        public MinecraftServerWrapper(string serverFile, string version)
        {
            ServerPath = new FileInfo(serverFile).DirectoryName!;
            ServerFile = serverFile;
            Version = version;
        }

        public void Start()
        {
            ProcessStartInfo startInfo = new("java", "-Xmx1024M -Xms1024M -jar " + ServerFile + " nogui")
            {
                WorkingDirectory = ServerPath,
                RedirectStandardInput = true,
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

        public async Task StopAsync()
        {
            await ServerProc.StandardInput.WriteLineAsync("stop");
            await ServerProc.WaitForExitAsync();
        }


    }
}
