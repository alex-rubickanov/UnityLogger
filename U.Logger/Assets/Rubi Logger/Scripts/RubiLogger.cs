using System;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Rubickanov.Logger
{
    public class RubiLogger : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private readonly static string DEFAULT_PATH = "Logs/log.txt";

        [SerializeField, Tooltip("Show logs in the console")]
        private bool showLogs = true;
        [SerializeField, Tooltip("Prefix for the logs")]
        private string categoryName = "Logger";
        [SerializeField, Tooltip("Color of the prefix")]
        private Color categoryColor = new Color(9, 167, 217, 255);
        [SerializeField,
         Tooltip("Log level filter. Only logs with the same or higher level will be shown in the console.")]
        private LogLevel logLevelFilter = LogLevel.Info;
        [FormerlySerializedAs("logFileFolder")] [SerializeField, Tooltip("Path to the log folder.")]
        private string logFilePath = "Game Logs/log.txt";

        [SerializeField] private bool screenLogsEnabled = false;
        [Tooltip("Show error message when trying to log to the screen when it's disabled.")]
        [SerializeField] private bool showErrorWhenDisabledScreenLogs = true;
        [SerializeField] private bool fileLogsEnabled = false;
        [Tooltip("Show error message when trying to log to a file when it's disabled.")]
        [SerializeField] private bool showErrorWhenDisabledFileLogs = true;


        public delegate void LogAddedHandler(string message);
        public event LogAddedHandler LogAdded;

        private void Awake()
        {
            ValidateLogFilePath();
        }

        private void OnValidate()
        {
            ValidatePrefixColor();
        }

        public void Log(LogLevel logLevel, object message, Object sender, LogOutput logOutput = LogOutput.Console,
            bool bypassLogLevelFilter = false)
        {
            if (ShouldLogMessage(logLevel, bypassLogLevelFilter))
            {
                string generatedMessage =
                    LogMessageGenerator.GenerateLogMessage(logLevel, message, categoryName, categoryColor, sender);

                switch (logOutput)
                {
                    case LogOutput.Console:
                        ConsoleLogMessage(logLevel, generatedMessage, sender);
                        break;

                    case LogOutput.Screen:
                        InvokeLogAddedEvent(generatedMessage);
                        break;

                    case LogOutput.File:
                        WriteToFileAsync(logLevel, message, sender);
                        break;

                    case LogOutput.ConsoleAndScreen:
                        ConsoleLogMessage(logLevel, generatedMessage, sender);
                        InvokeLogAddedEvent(generatedMessage);
                        break;

                    case LogOutput.ConsoleAndFile:
                        ConsoleLogMessage(logLevel, generatedMessage, sender);
                        WriteToFileAsync(logLevel, message, sender);
                        break;

                    case LogOutput.ScreenAndFile:
                        InvokeLogAddedEvent(generatedMessage);
                        WriteToFileAsync(logLevel, message, sender);
                        break;

                    case LogOutput.All:
                        ConsoleLogMessage(logLevel, generatedMessage, sender);
                        InvokeLogAddedEvent(generatedMessage);
                        WriteToFileAsync(logLevel, message, sender);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(logOutput), logOutput, null);
                }
            }
        }

        private async void WriteToFileAsync(LogLevel logLevel, object message, Object sender)
        {
            if (!fileLogsEnabled)
            {
                if (showErrorWhenDisabledFileLogs)
                {
                    Log(LogLevel.Error, "File logs are disabled. Enable them in the Logger component.", this,
                        LogOutput.Console, true);
                }

                return;
            }

            string fileLog = LogMessageGenerator.GenerateFileLog(logLevel, message, categoryName, sender);
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
            if (!ColorUtility.TryParseHtmlString("#" + ColorUtility.ToHtmlStringRGB(categoryColor), out var newColor))
            {
                Debug.LogError(
                    "Invalid color string. Please enter a valid color string in the format #RRGGBB or #RGB.");
            }
            else
            {
                categoryColor = newColor;
            }
        }

        private bool ShouldLogMessage(LogLevel logLevel, bool bypassLogLevelFilter)
        {
            return (showLogs && logLevel >= logLevelFilter) || bypassLogLevelFilter;
        }


        private void ConsoleLogMessage(LogLevel logLevel, string message, Object sender)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    Debug.Log(message, sender);
                    break;
                case LogLevel.Warn:
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
                if (showErrorWhenDisabledScreenLogs)
                {
                    Log(LogLevel.Error, "Screen logs are disabled. Enable them in the Logger component.", this,
                        LogOutput.Console, true);
                }

                return;
            }

            LogAdded?.Invoke(message);
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
    }
}