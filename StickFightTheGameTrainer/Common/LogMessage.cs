using System;

namespace StickFightTheGameTrainer.Common
{
    public class LogMessage
    {
        public readonly string Message;
        public readonly LogLevel LogLevel;
        public readonly DateTime DateCreated;

        public LogMessage(string message, LogLevel logLevel = LogLevel.Info)
        {
            Message = message;
            LogLevel = logLevel;
            DateCreated = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{DateCreated}] [{Enum.GetName(typeof(LogLevel), LogLevel)}] - {Message}";
        }
    }
}
