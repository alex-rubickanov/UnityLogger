using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rubickanov.Logger
{
    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    public class Logger : MonoBehaviour
    {
        [SerializeField, HideInInspector]
#pragma warning disable CS0414 // Field is assigned but its value is never used
        private string DEFAULT_PATH = "Logs/log.txt";
#pragma warning restore CS0414 // Field is assigned but its value is never used

        [Header("Settings")]
        [SerializeField, Tooltip("Show logs in the console")]
        private bool showLogs = true;
        [SerializeField, Tooltip("Prefix for the logs")]
        private string prefix = "Logger";
        [SerializeField, Tooltip("Color of the prefix")]
        private Color prefixColor;
        [SerializeField, Tooltip("Log level filter")]
        private LogType logLevelFilter = LogType.Info;
        [SerializeField, Tooltip("Path to the log file")]
        private string logFilePath = "Logs/log.txt";

        private string hexColor;
        private Color lastColor;

        private StreamWriter logFile;

        private void OnValidate()
        {
            if (lastColor != prefixColor)
            {
                hexColor = "#" + ColorUtility.ToHtmlStringRGB(prefixColor);
                lastColor = prefixColor;
            }
        }

        public void Log(LogType logType, string message, Object sender, bool bypassLogLevelFilter = false,
            bool writeToFile = false)
        {
            if (showLogs && logType >= logLevelFilter || bypassLogLevelFilter)
            {
                string generatedMessage = $"<color={hexColor}>[{prefix}] </color> [{sender.name}]: {message}";

                switch (logType)
                {
                    case LogType.Info:
                        Debug.Log(generatedMessage, sender);
                        break;
                    case LogType.Warning:
                        Debug.LogWarning(generatedMessage, sender);
                        break;
                    case LogType.Error:
                        Debug.LogError(generatedMessage, sender);
                        break;
                }

                if (writeToFile)
                {
                    WriteToFile(message);
                }
            }
        }

        private void WriteToFile(string message)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));

            logFile ??= new StreamWriter(logFilePath, true);

            logFile.WriteLine(message);
            logFile.Flush();
            logFile.Close();
        }
    }
}