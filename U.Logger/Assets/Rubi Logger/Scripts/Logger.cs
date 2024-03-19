using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rubickanov.Logger
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public enum LogType
    {
        Console,
        Screen,
        File,
        ConsoleAndScreen,
        ConsoleAndFile,
        ScreenAndFile,
        All
    }

    public class Logger : MonoBehaviour
    {
        // MAIN SETTINGS
        [SerializeField, Tooltip("Show logs in the console.")]
        private bool showLogs = true;
        [SerializeField, Tooltip("Prefix for the logs.")]
        private string prefix = "Logger";
        [SerializeField, Tooltip("Color of the prefix.")]
        private Color prefixColor = new Color(9, 167, 217, 255);
        [SerializeField,
         Tooltip("Log level filter. Only logs with the same or higher level will be shown in the console.")]
        private LogLevel logLevelFilter = LogLevel.Info;
        [SerializeField, Tooltip("Format of the log message.")]
        private LogFormat logFormat;

        [SerializeField] private bool screenLogsEnabled = false;
        [SerializeField] private bool fileLogsEnabled = false;

        // SCREEN LOG SETTINGS
        [SerializeField]
        private int fontSize = 14;
        [SerializeField]
        private float logLifetime = 3.0f;

        // FILE LOG SETTINGS
        [SerializeField, HideInInspector]
        private string DEFAULT_PATH = "Game Logs/log.txt";
        [SerializeField, Tooltip("Path to the log file.")]
        private string logFilePath = "Game Logs/log.txt";

        public delegate void LogAddedHandler(string message);

        public event LogAddedHandler LogAdded;

        private static int logIndex = 1;

        private readonly Dictionary<LogLevel, string> logTypeColors = new Dictionary<LogLevel, string>
        {
            { LogLevel.Info, "#FFFFFF" },
            { LogLevel.Warning, "#FFFF00" },
            { LogLevel.Error, "#FF0000" }
        };

        private LRUCache<string, string> cachedStrings = new LRUCache<string, string>(100);

        private void Awake()
        {
            ValidateLogFilePath();
        }

        private void OnValidate()
        {
            ValidatePrefixColor();
        }

        public void Log(LogLevel logLevel, string message, Object sender, LogType logType = LogType.Console,
            bool bypassLogLevelFilter = false)
        {
            if (ShouldLogMessage(logLevel, bypassLogLevelFilter))
            {
                string generatedMessage;

                switch (logType)
                {
                    case LogType.Console:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender, logFormat.ConsoleFormat);
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        break;

                    case LogType.Screen:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender, logFormat.ScreenFormat);
                        InvokeLogAddedEvent(generatedMessage);
                        break;

                    case LogType.File:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender, logFormat.FileFormat, true);
                        WriteToFileAsync(logLevel, generatedMessage, sender);
                        break;

                    case LogType.ConsoleAndFile:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender, logFormat.ConsoleFormat);
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        generatedMessage = GenerateLogMessage(logLevel, message, sender, logFormat.FileFormat, true);
                        WriteToFileAsync(logLevel, generatedMessage, sender);
                        break;

                    case LogType.ScreenAndFile:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender, logFormat.ScreenFormat);
                        InvokeLogAddedEvent(generatedMessage);
                        generatedMessage = GenerateLogMessage(logLevel, message, sender, logFormat.FileFormat, true);
                        WriteToFileAsync(logLevel, generatedMessage, sender);
                        break;

                    case LogType.All:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender, logFormat.ConsoleFormat);
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        generatedMessage = GenerateLogMessage(logLevel, message, sender, logFormat.ScreenFormat);
                        InvokeLogAddedEvent(generatedMessage);
                        generatedMessage = GenerateLogMessage(logLevel, message, sender, logFormat.FileFormat, true);
                        WriteToFileAsync(logLevel, generatedMessage, sender);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
                }
            }
        }

        private async void WriteToFileAsync(LogLevel logLevel, string message, Object sender)
        {
            string fileLog = GenerateFileLog(logLevel, message, sender);
            await WriteLogToFile(fileLog);
        }

        private void ValidateLogFilePath()
        {
            if (string.IsNullOrEmpty(logFilePath))
            {
                logFilePath = DEFAULT_PATH;
            }
        }

        private void ValidatePrefixColor()
        {
            if (!ColorUtility.TryParseHtmlString("#" + ColorUtility.ToHtmlStringRGB(prefixColor), out var newColor))
            {
                Debug.LogError(
                    "Invalid color string. Please enter a valid color string in the format #RRGGBB or #RGB.");
            }
            else
            {
                prefixColor = newColor;
            }
        }

        private bool ShouldLogMessage(LogLevel logLevel, bool bypassLogLevelFilter)
        {
            return (showLogs && logLevel >= logLevelFilter) || bypassLogLevelFilter;
        }

        private string GenerateLogMessage(LogLevel logLevel, string message, Object sender, List<LogPart> logParts,
            bool isForFile = false)
        {
            string cacheKey = $"{logLevel}_{sender.name}_{prefix}_{message}_{isForFile}";
            string cachedMessage = cachedStrings.Get(cacheKey);

            if (cachedMessage != null)
            {
                return cachedMessage;
            }

            string logTypeColor = GetCachedString($"logTypeColor_{logLevel}", () => logTypeColors[logLevel]);
            string hexColor = GetCachedString("prefixColor", () => "#" + ColorUtility.ToHtmlStringRGB(prefixColor));
            StringBuilder generatedMessage = new StringBuilder();

            foreach (var part in logParts)
            {
                string partMessage = part switch
                {
                    LogPart.DateTime => GetCachedString("dateTime", () => DateTime.Now.ToString()) + " ",
                    LogPart.TimeFromStart =>
                        GetCachedString("timeFromStart", () => Time.timeSinceLevelLoad.ToString()) + " ",
                    LogPart.LogLevel => isForFile ? $"[{logLevel}] " : $"<color={logTypeColor}>[{logLevel}]</color> ",
                    LogPart.SenderName => $"[{sender.name}] ",
                    LogPart.Prefix => isForFile ? $"[{prefix}] " : $"<color={hexColor}>[{prefix}]</color> ",
                    LogPart.Message => message + " ",
                    LogPart.Index => $"[{logIndex}] ",
                    _ => throw new ArgumentOutOfRangeException(nameof(part), part, null)
                };

                generatedMessage.Append(partMessage);
            }

            string newMessage = generatedMessage.ToString().TrimEnd();
            cachedStrings.Add(cacheKey, newMessage);

            return newMessage;
        }

        private void DisplayLogMessage(LogLevel logLevel, string message, Object sender)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    Debug.Log(message, sender);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message, sender);
                    break;
                case LogLevel.Error:
                    Debug.LogError(message, sender);
                    break;
                default:
                    Debug.Log(message, sender);
                    break;
            }
        }

        private void InvokeLogAddedEvent(string message)
        {
            LogAdded?.Invoke(message);
        }

        private string GenerateFileLog(LogLevel logLevel, string message, Object sender)
        {
            return $"{DateTime.Now} [{logLevel}] [{prefix}] [{sender.name}]: {message}";
        }

        private async System.Threading.Tasks.Task WriteLogToFile(string message)
        {
            string directoryPath = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (StreamWriter logFile = new StreamWriter(logFilePath, true))
            {
                await logFile.WriteLineAsync(message);
            }
        }

        public string GetLogTypeColor(LogLevel logLevel)
        {
            return logTypeColors[logLevel];
        }

        private string GetCachedString(string key, Func<string> generateString)
        {
            var cachedString = cachedStrings.Get(key);
            if (cachedString == null)
            {
                cachedString = generateString();
                cachedStrings.Add(key, cachedString);
            }

            return cachedString;
        }
    }
}