using System;
using System.Collections.Generic;

namespace GSLogger
{
    public interface IDebugListener
    {
        LogLevel LogLevel { get; set; }
        bool DuplicateToConsole { get; set; }
        void LogDebug(string message);
        void LogInfo(string message);
        
        void LogError(Exception ex);
        void LogError(string message);

        void LogWarning(Exception ex);
        void LogWarning(string message);
    }

    public interface IDebugCrashDumper
    {
        void LogCrashDump(string sender, string version, IEnumerable<LogEntry> entries);
    }

    public enum LogLevel
    {
        Silent=0, Error=1, Warning=2, Info=3, Debug=4
    }
}
