using System.Collections.Generic;
using UnityEngine;

namespace Rubickanov.Logger
{
    [CreateAssetMenu(fileName = "LogFormat", menuName = "Rubi Logger/Log Format", order = 0)]
    public class LogFormat : ScriptableObject
    {
        [SerializeField]
        private List<LogPart> consoleLogFormat = new List<LogPart>();
        [SerializeField]
        private List<LogPart> screenLogFormat = new List<LogPart>();
        [SerializeField]
        private List<LogPart> fileLogFormat = new List<LogPart>();

        public List<LogPart> ConsoleFormat => consoleLogFormat;
        public List<LogPart> ScreenFormat => screenLogFormat;
        public List<LogPart> FileFormat => fileLogFormat;
    }

    public enum LogPart
    {
        DateTime,
        TimeFromStart,
        LogLevel,
        SenderName,
        Prefix,
        Message,
        Index
    }
}
