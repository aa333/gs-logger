using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSLogger
{
    public class TextLogger : IDebugListener, IDebugCrashDumper
    {
        private const int MaxFilesCount = 5;
        readonly string _workDir;
        string _workFilePath;
        private const int MaxFileLength = 1048576;
        private string _logPreffix = "Log_";
        public string LogPreffix
        {
            get { return _logPreffix; }
            set
            {
                File.Delete(_workFilePath);
                _logPreffix = value;
                _workFilePath = CreateNewFile(_logPreffix, _workDir);
            }
        }
        public bool DuplicateToConsole { get; set; }
        public string CrashPreffix { get; set; } = "Crash_";

        readonly object _locker = new object();
        StreamWriter _sw;
        int _fileNum;
        LogLevel _logLevel = LogLevel.Info;
        public LogLevel LogLevel
        {
            get { lock (_locker) return _logLevel; }
            set { lock (_locker) _logLevel = value; }
        }
        public string TimeStamp => DateTime.Now.ToString("G");

        public TextLogger(string directory)
        {
            Cleanup();
            _workDir = directory;
            _workFilePath = CreateNewFile(LogPreffix, directory);
        }

        private string CreateNewFile(string filePreffix, string filePath)
        {
            var path = "";
            try
            {
                var fullName = filePreffix + DateTime.Now.ToString("dd.MM.yyyy") + " at " + DateTime.Now.ToString("HH-mm-ss") +
                        ".txt";
                path = Path.Combine(String.IsNullOrEmpty(filePath) ? Directory.GetCurrentDirectory() : filePath,
                     fullName);
            }
            catch (Exception ex)
            {
                LogLevel = LogLevel.Silent;
                Log.Error(ex);
            }
            return path;
        }

        private void Cleanup()
        {
            try
            {
                // Get the files
                var info = new DirectoryInfo(Directory.GetCurrentDirectory());
                var filesL = new List<FileInfo>(info.GetFiles(LogPreffix + "*.txt"));
                var filesC = new List<FileInfo>(info.GetFiles(CrashPreffix + "*.txt"));

                // Sort by creation-time ascending 
                filesL.Sort((f1, f2) => f1.CreationTime.CompareTo(f2.CreationTime));
                filesC.Sort((f1, f2) => f1.CreationTime.CompareTo(f2.CreationTime));

                while (filesL.Count > MaxFilesCount)
                {
                    File.Delete(filesL[0].FullName);
                    filesL.RemoveAt(0);
                }

                while (filesC.Count > MaxFilesCount)
                {
                    File.Delete(filesC[0].FullName);
                    filesC.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                LogLevel = LogLevel.Silent;
                Log.Error(ex);
            }
        }

        private string FormatString(string message, string level)
        {
            return $"{TimeStamp,-20} {level,-5}  {message}" +Environment.NewLine;
        }
        private void WriteToConsole(string ms)
        {
            if(DuplicateToConsole)
                Console.WriteLine(ms);
        }

        public void LogDebug(string message)
        {
            lock (_locker)
            {
                if (_logLevel>=LogLevel.Debug)
                {
                    var ms = FormatString(message, "DEBUG");
                    WriteToConsole(ms);
                    WriteToFile(ms);
                }
            }
        }

        public void LogInfo(string message)
        {
            lock (_locker)
            {
                if (_logLevel >= LogLevel.Info)
                {
                    string ms = FormatString(message, "INFO");
                    WriteToConsole(ms);
                    WriteToFile(ms);
                }
            }
        }

        public void LogWarning(Exception ex)
        {
            lock (_locker)
            {
                if (_logLevel >= LogLevel.Warning)
                {
                    string ms = FormatString(ex.ToString(), "WARN");
                    WriteToConsole(ms);
                    WriteToFile(ms);
                }
            }
        }

        public void LogWarning(string message)
        {
            lock (_locker)
            {
                if (_logLevel >= LogLevel.Warning)
                {
                    string ms = FormatString(message, "WARN");
                    WriteToConsole(ms);
                    WriteToFile(ms);
                }
            }
        }

        public void LogError(Exception ex)
        {
            lock (_locker)
            {
                if (_logLevel >= LogLevel.Error)
                {
                    string ms = FormatString(ex.ToString(), "ERROR");
                    WriteToConsole(ms);
                    WriteToFile(ms);
                }
            }
        }
        public void LogError(string message)
        {
            lock (_locker)
            {
                if (_logLevel >= LogLevel.Error)
                {
                    string ms = FormatString(message, "ERROR");
                    WriteToConsole(ms);
                    WriteToFile(ms);
                }
            }
        }

        private void WriteToFile(string message)
        {
            WriteToFile(message,_workFilePath);
            var fi = new FileInfo(_workFilePath);
            if (fi.Length > MaxFileLength)
            {
                _fileNum++;
                _workFilePath = CreateNewFile(LogPreffix + _fileNum + "_", _workDir);
            }
            Cleanup();
        }

        private void WriteToFile(string message, string path)
        {
            try
            {
                _sw = new StreamWriter(path, true);
                _sw.Write(message);
                _sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logLevel = LogLevel.Silent;
            }
        }
        
        public void LogCrashDump(string sender, string version, IEnumerable<LogEntry> entries)
        {
            var path = CreateNewFile(CrashPreffix,_workDir);
            var dump = new StringBuilder();
            dump.Append(sender+" ver. "+version+ " CRASH DUMP");
            dump.Append(Environment.NewLine);
            dump.Append("===========================================");
            dump.Append(Environment.NewLine);
            dump.Append(Environment.NewLine);
            foreach (var entry in entries)
            {
                switch (entry.Level)
                {
                    case LogLevel.Debug:
                        dump.Append(FormatString(entry.Entry, "DEBUG"));
                        break;
                    case LogLevel.Info:
                        dump.Append(FormatString(entry.Entry, "INFO"));
                        break;
                    case LogLevel.Warning:
                        if (!String.IsNullOrEmpty(entry.Entry))
                            dump.Append(FormatString(entry.Entry, "WARN"));
                        if (entry.Exception!=null)
                            dump.Append(FormatString(entry.Exception.ToString(), "WARN"));
                        break;
                    case LogLevel.Error:
                        if (!String.IsNullOrEmpty(entry.Entry))
                            dump.Append(FormatString(entry.Entry, "ERROR"));
                        if (entry.Exception!=null)
                            dump.Append(FormatString(entry.Exception.ToString(), "ERROR"));
                        break;
                }
            }
            WriteToFile(dump.ToString(),path);
        }
    }

   
}
