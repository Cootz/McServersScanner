using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.IO;

public static class AppDirectoryManager
{
    private static string appName = "McServerScanner";
    private static string data = "data";

    public static string AppData { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName); } }
    public static string Data { get { return Path.Combine(AppData, data); } }

    public static async Task Initialize()
    {
        ensureDirectoryCreated(AppData);
        ensureDirectoryCreated(Data);
    }

    private static void ensureDirectoryCreated(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
