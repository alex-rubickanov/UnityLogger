using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Rubickanov.Logger
{
    public static class FileLogWriter
    {
        private static ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
        private static bool isWriting = false;

        public static void FileLog(string message, string path)
        {
            logQueue.Enqueue(message);
            WriteLogsToFile(path);
        }

        private static async void WriteLogsToFile(string path)
        {
            if (isWriting) return;

            isWriting = true;

            await Task.Run(async () =>
            {
                while (logQueue.TryDequeue(out string message))
                {
                    string directoryPath = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    using (StreamWriter logFile = new StreamWriter(path, true))
                    {
                        await logFile.WriteLineAsync(message);
                    }
                }
            });

            isWriting = false;
        }
    }
}