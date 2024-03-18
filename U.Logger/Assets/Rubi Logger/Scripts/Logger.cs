using System;
using System.Collections.Generic;
using System.IO;
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
        [SerializeField, HideInInspector]
        private string DEFAULT_PATH = "Game Logs/log.txt";

        [SerializeField, Tooltip("Show logs in the console")]
        private bool showLogs = true;
        [SerializeField, Tooltip("Prefix for the logs")]
        private string prefix = "Logger";
        [SerializeField, Tooltip("Color of the prefix")]
        private Color prefixColor = new Color(9, 167, 217, 255);
        [SerializeField,
         Tooltip("Log level filter. Only logs with the same or higher level will be shown in the console.")]
        private LogLevel logLevelFilter = LogLevel.Info;
        [SerializeField, Tooltip("Path to the log file")]
        private string logFilePath = "Game Logs/log.txt";

        public delegate void LogAddedHandler(string message);

        public event LogAddedHandler LogAdded;

        private readonly Dictionary<LogLevel, string> logTypeColors = new Dictionary<LogLevel, string>
        {
            { LogLevel.Info, "#FFFFFF" },
            { LogLevel.Warning, "#FFFF00" },
            { LogLevel.Error, "#FF0000" }
        };

        private void Awake()
        {
            ValidateLogFilePath();
        }

        private void OnValidate()
        {
            ValidatePrefixColor();
        }

        public void Log(LogLevel logLevel, string message, Object sender, LogType logType = LogType.Console, bool bypassLogLevelFilter = false)
        {
            if (ShouldLogMessage(logLevel, bypassLogLevelFilter))
            {
                string generatedMessage = GenerateLogMessage(logLevel, message, sender);

                switch (logType)
                {
                    case LogType.Console:
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        break;
                    
                    case LogType.Screen:
                        InvokeLogAddedEvent(generatedMessage);
                        break;
                    
                    case LogType.File:
                        WriteToFileAsync(logLevel, message, sender);
                        break;
                    
                    case LogType.ConsoleAndScreen:
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        InvokeLogAddedEvent(generatedMessage);
                        break;
                    
                    case LogType.ConsoleAndFile:
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        WriteToFileAsync(logLevel, message, sender);
                        break;
                    
                    case LogType.ScreenAndFile:
                        InvokeLogAddedEvent(generatedMessage);
                        WriteToFileAsync(logLevel, message, sender);
                        break;
                    
                    case LogType.All:
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        InvokeLogAddedEvent(generatedMessage);
                        WriteToFileAsync(logLevel, message, sender);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
                }
            }
        }

        public void ScreenLog(LogLevel logLevel, string message, Object sender)
        {
            string generatedMessage = GenerateLogMessage(logLevel, message, sender);
            DisplayLogMessage(logLevel, generatedMessage, sender);
            InvokeLogAddedEvent(generatedMessage);
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

        private string GenerateLogMessage(LogLevel logLevel, string message, Object sender)
        {
            string logTypeColor = logTypeColors[logLevel];
            string hexColor = "#" + ColorUtility.ToHtmlStringRGB(prefixColor);
            return
                $"<color={logTypeColor}>[{logLevel}]</color> <color={hexColor}>[{prefix}] </color> [{sender.name}]: {message}";
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
    }
}