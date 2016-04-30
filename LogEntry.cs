using System;

namespace GSLogger
{
    public struct LogEntry
    {
        public string Entry;
        public DateTime Time;
        public LogLevel Level;
        public Exception Exception;
        public LogEntry(string entry, DateTime time, LogLevel level, Exception exception = null)
        {
            Level = level;
            Entry = entry;
            Time = time;
            Exception = exception;
        }
    }
}
