namespace StartProject.Asp;

using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Hosting;

public class StartUpHook
{
    string[] _folderPath = ["db", "Config", "MediaServer", "Orders"];

    public void EnsureFileExist()
    {
        foreach (var path in _folderPath)
        {
            var dbPath = path;
            var backDbPath = Path.Combine("Backups", path);
            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
            }
            var dbFiles = Directory.GetFiles(dbPath).Where(fn => !fn.Contains(".gitkeep")).ToArray();
            var dbFolders = Directory.GetDirectories(dbPath);
            if (dbFiles.Length == 0 && dbFolders.Length == 0)
            {
                var backFiles = Directory.GetFiles(backDbPath);
                foreach (var file in backFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var targetFile = Path.Combine(dbPath, fileName);
                    File.Copy(file, targetFile);
                }
                var backFolders = Directory.GetDirectories(backDbPath);
                foreach (var folder in backFolders)
                {
                    var folderName = Path.GetFileName(folder);
                    var folderFile = Directory.GetFiles(folder);
                    foreach (var file in folderFile)
                    {
                        var fileFolName = Path.GetFileName(file);
                        Directory.CreateDirectory(dbPath + "/" + folderName);
                        var targetFolName = Path.Combine(dbPath, folderName, fileFolName);
                        File.Copy(file, targetFolName);
                    }
                }
            }
        }
    }
}
