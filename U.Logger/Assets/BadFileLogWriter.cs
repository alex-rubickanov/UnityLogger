using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class BadFileLogWriter
{
    private static Queue<string> logQueue = new Queue<string>();
    private static bool isWriting = false;

    public static void FileLog(string message, string path)
    {
        WriteLogsToFile(message, path);
    }

    private static void WriteLogsToFile(string message, string path)
    {
        string directoryPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using (StreamWriter logFile = new StreamWriter(path, true))
        {
            logFile.WriteLineAsync(message);
        }
    }
}