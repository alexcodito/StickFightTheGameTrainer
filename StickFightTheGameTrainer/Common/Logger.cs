using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Common
{
    public class Logger : ILogger
    {
        private readonly IList<LogMessage> _currentLogs;
        public event Action<LogMessage> NewLogEvent;
        public static readonly Task CompletedTask = Task.FromResult(0);

        public bool HasErrors => _currentLogs?.Any(log => log.LogLevel == LogLevel.Error) == true;

        public Logger()
        {
            _currentLogs = new List<LogMessage>();
        }

        public async Task Log(string message, LogLevel logLevel = LogLevel.Info)
        {
            LogMessage newLogMessage = null;
            await Task.Run(() =>
            {
                newLogMessage = new LogMessage(message, logLevel);
                _currentLogs.Add(newLogMessage);
            });

            NewLogEvent?.Invoke(newLogMessage);
        }

        public void RemoveLog(LogMessage logMessage)
        {
            _currentLogs.Remove(logMessage);
        }

        public void RemoveLog(int index)
        {
            _currentLogs.RemoveAt(index);
        }

        public LogMessage Pop()
        {
            if (!_currentLogs.Any())
            {
                return null;
            }

            var latest = _currentLogs.Last();
            _currentLogs.RemoveAt(_currentLogs.Count);
            return latest;
        }

        public void ClearLogs()
        {
            _currentLogs.Clear();
        }

        public IList<LogMessage> GetLogs()
        {
            return _currentLogs;
        }
    }
}
