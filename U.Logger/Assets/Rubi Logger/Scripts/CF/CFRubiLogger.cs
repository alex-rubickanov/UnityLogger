using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rubickanov.Logger
{
    public class CFRubiLogger : RubiLogger
    {
        // MAIN SETTINGS
        [SerializeField, Tooltip("Format of the log message.")]
        private LogFormat logFormat;

        public LogFormat LogFormat => logFormat;

        // SCREEN LOG SETTINGS

        // FILE LOG SETTINGS
        
        public event LogAddedHandler LogAddedCF;

        private static int logIndex = 0;

        // Unity Methods
        private void Awake()
        {
            if (logFormat == null)
            {
                Debug.LogError("Log format is not set. Please set the log format in the Logger component.");
            }

            ValidateLogFilePath();
        }

        private void OnValidate()
        {
            ValidatePrefixColor();
        }

        // Public Methods
        public override void Log(LogLevel logLevel, string message, Object sender, LogOutput logOutput = LogOutput.Console,
            bool bypassLogLevelFilter = false)
        {
            if (ShouldLogMessage(logLevel, bypassLogLevelFilter))
            {
                logIndex++;

                string generatedMessage;

                switch (logOutput)
                {
                    case LogOutput.Console:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender.name, logFormat.ConsoleFormat);
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        break;

                    case LogOutput.Screen:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender.name, logFormat.ScreenFormat);
                        InvokeLogAddedEvent(generatedMessage);
                        break;

                    case LogOutput.File:
                        generatedMessage =
                            GenerateLogMessage(logLevel, message, sender.name, logFormat.FileFormat, false);
                        WriteToFileAsync(generatedMessage);
                        break;

                    case LogOutput.ConsoleAndScreen:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender.name, logFormat.ConsoleFormat);
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        generatedMessage = GenerateLogMessage(logLevel, message, sender.name, logFormat.ScreenFormat);
                        InvokeLogAddedEvent(generatedMessage);
                        break;

                    case LogOutput.ConsoleAndFile:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender.name, logFormat.ConsoleFormat);
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        generatedMessage =
                            GenerateLogMessage(logLevel, message, sender.name, logFormat.FileFormat, false);
                        WriteToFileAsync(generatedMessage);
                        break;

                    case LogOutput.ScreenAndFile:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender.name, logFormat.ScreenFormat);
                        InvokeLogAddedEvent(generatedMessage);
                        generatedMessage =
                            GenerateLogMessage(logLevel, message, sender.name, logFormat.FileFormat, false);
                        WriteToFileAsync(generatedMessage);
                        break;

                    case LogOutput.All:
                        generatedMessage = GenerateLogMessage(logLevel, message, sender.name, logFormat.ConsoleFormat);
                        DisplayLogMessage(logLevel, generatedMessage, sender);
                        generatedMessage = GenerateLogMessage(logLevel, message, sender.name, logFormat.ScreenFormat);
                        InvokeLogAddedEvent(generatedMessage);
                        generatedMessage =
                            GenerateLogMessage(logLevel, message, sender.name, logFormat.FileFormat, false);
                        WriteToFileAsync(generatedMessage);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(logOutput), logOutput, null);
                }
            }
        }

        // Private Methods
        private async void WriteToFileAsync(string log)
        {
            if (!fileLogsEnabled)
            {
                Log(LogLevel.Warning, "File logs are disabled. Enable them in the Logger component.", this,
                    LogOutput.Console);
                return;
            }

            await WriteLogToFile(log);
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

        private string GenerateLogMessage(LogLevel logLevel, string message, string senderName, List<LogPart> logParts, bool useColor = true)
        {
            StringBuilder generatedMessage = new StringBuilder();

            foreach (var part in logParts)
            {
                string partMessage = part switch
                {
                    LogPart.Index => $"[{logIndex}] ",
                    LogPart.DateTime => DateTime.Now.ToString() + " ",
                    LogPart.TimeFromStart => Time.timeSinceLevelLoad.ToString() + " ",
                    LogPart.LogLevel => useColor
                        ? $"<color={logTypeColors[logLevel]}>[{logLevel}]</color> "
                        : $"[{logLevel}] ",
                    LogPart.SenderName => $"[{senderName}] ",
                    LogPart.Prefix => useColor
                        ? $"<color={"#" + ColorUtility.ToHtmlStringRGB(prefixColor)}>[{prefix}]</color> "
                        : $"[{prefix}] ",
                    LogPart.Message => message + " ",
                    _ => throw new ArgumentOutOfRangeException(nameof(part), part, null)
                };

                generatedMessage.Append(partMessage);
            }

            return generatedMessage.ToString().TrimEnd();
        }

        private string GenerateLogMessage(LogLevel logLevel, string message, Object sender, List<LogPart> logParts, bool useColor = true)
        {
            StringBuilder generatedMessage = new StringBuilder();

            foreach (var part in logParts)
            {
                string partMessage = part switch
                {
                    LogPart.Index => $"[{logIndex}] ",
                    LogPart.DateTime => DateTime.Now.ToString() + " ",
                    LogPart.TimeFromStart => Time.timeSinceLevelLoad.ToString() + " ",
                    LogPart.LogLevel => useColor
                        ? $"<color={logTypeColors[logLevel]}>[{logLevel}]</color> "
                        : $"[{logLevel}] ",
                    LogPart.SenderName => $"[{sender.name}] ",
                    LogPart.Prefix => useColor
                        ? $"<color={"#" + ColorUtility.ToHtmlStringRGB(prefixColor)}>[{prefix}]</color> "
                        : $"[{prefix}] ",
                    LogPart.Message => message + " ",
                    _ => throw new ArgumentOutOfRangeException(nameof(part), part, null)
                };

                generatedMessage.Append(partMessage);
            }

            return generatedMessage.ToString().TrimEnd();
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

            LogAddedCF?.Invoke(message);
        }

        [Tooltip("Method used for editor preview.")]
        public string GenerateLogPreview(LogLevel logType, List<LogPart> logParts, bool useColor = true)
        {
            string message = "This is a " + logType + " message";
            string senderName = "SenderName";
            return GenerateLogMessage(logType, message, senderName, logParts, useColor);
        }
    }
}