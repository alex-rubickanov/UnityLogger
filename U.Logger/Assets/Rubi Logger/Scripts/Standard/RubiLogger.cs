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

    public enum LogOutput
    {
        Console,
        Screen,
        File,
        ConsoleAndScreen,
        ConsoleAndFile,
        ScreenAndFile,
        All
    }

    public class RubiLogger : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        protected string DEFAULT_PATH = "Game Logs/log.txt";

        [SerializeField, Tooltip("Show logs in the console")]
        protected bool showLogs = true;
        [SerializeField, Tooltip("Prefix for the logs")]
        protected string prefix = "Logger";
        [SerializeField, Tooltip("Color of the prefix")]
        protected Color prefixColor = new Color(9, 167, 217, 255);
        [SerializeField,
         Tooltip("Log level filter. Only logs with the same or higher level will be shown in the console.")]
        protected LogLevel logLevelFilter = LogLevel.Info;
        [SerializeField, Tooltip("Path to the log file")]
        protected string logFilePath = "Game Logs/log.txt";

        [SerializeField] protected bool screenLogsEnabled = false;
        [SerializeField] protected bool fileLogsEnabled = false;

        public delegate void LogAddedHandler(string message);

        public event LogAddedHandler LogAdded;

        protected readonly Dictionary<LogLevel, string> logTypeColors = new Dictionary<LogLevel, string>
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

        public virtual void Log(LogLevel logLevel, string message, Object sender, LogOutput logOutput = LogOutput.Console,
            bool bypassLogLevelFilter = false)
        {
            if (ShouldLogMessage(logLevel, bypassLogLevelFilter))
            {
                string generatedMessage = GenerateLogMessage(logLevel, message, sender);

                switch (logOutput)
                {
                    case LogOutput.Console:
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        break;

                    case LogOutput.Screen:
                        InvokeLogAddedEvent(generatedMessage);
                        break;

                    case LogOutput.File:
                        WriteToFileAsync(logLevel, message, sender);
                        break;

                    case LogOutput.ConsoleAndScreen:
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        InvokeLogAddedEvent(generatedMessage);
                        break;

                    case LogOutput.ConsoleAndFile:
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        WriteToFileAsync(logLevel, message, sender);
                        break;

                    case LogOutput.ScreenAndFile:
                        InvokeLogAddedEvent(generatedMessage);
                        WriteToFileAsync(logLevel, message, sender);
                        break;

                    case LogOutput.All:
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        InvokeLogAddedEvent(generatedMessage);
                        WriteToFileAsync(logLevel, message, sender);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(logOutput), logOutput, null);
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
            if (!screenLogsEnabled)
            {
                Log(LogLevel.Warning, "Screen logs are disabled. Enable them in the Logger component.", this);
                return;
            }

            LogAdded?.Invoke(message);
        }

        private string GenerateFileLog(LogLevel logLevel, string message, Object sender)
        {
            return $"{DateTime.Now} [{logLevel}] [{prefix}] [{sender.name}]: {message}";
        }

        protected async System.Threading.Tasks.Task WriteLogToFile(string message)
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