using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Common
{
    public interface ILogger
    {
        event Action<LogMessage> NewLogEvent;

        bool HasErrors { get; }

        void ClearLogs();

        IList<LogMessage> GetLogs();

        Task Log(string message, LogLevel logLevel = LogLevel.Info);

        LogMessage Pop();

        void RemoveLog(LogMessage logMessage);

        void RemoveLog(int index);
    }
}
