using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSLogger
{
    public class HtmlLogger : IDebugListener, IDebugCrashDumper
    {
        private const int MaxFilesCount = 5;
        LogLevel _logLevel = LogLevel.Info;
        readonly string _workDir;
        readonly string _workFilePath;
        const string LogPreffix = "HTMLog_";
        const string CrashPreffix = "Oops_";
        readonly object locker = new object();
        StreamWriter sw;
        int _errorStackId;

        public LogLevel LogLevel
        {
            get { lock (locker) return _logLevel; }
            set { lock (locker) _logLevel = value; }
        }

        public bool DuplicateToConsole { get; set; }
        public string TimeStamp => DateTime.Now.ToString("G");


        public HtmlLogger(string directory)
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
                        ".html";
                path = Path.Combine(String.IsNullOrEmpty(filePath) ? Directory.GetCurrentDirectory() : filePath,
                     fullName);
                sw = new StreamWriter(path);
                sw.WriteLine("<html>");
                sw.WriteLine("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");

                //colors
                sw.WriteLine(@"<style type=""text/css"">" + Environment.NewLine
                             + ".debug{color:#" + GsColor.Debug.ToHexadecimal() + "}" + Environment.NewLine
                             + ".info{color:#" + GsColor.Info.ToHexadecimal() + "}" + Environment.NewLine
                             + ".warning{color:#" + GsColor.Warning.ToHexadecimal() + "}" + Environment.NewLine
                             + ".error{color:#" + GsColor.Error.ToHexadecimal() + "}" + Environment.NewLine
                             + ".stacktrace{color:#" + GsColor.Maroon.ToHexadecimal() + "}" + Environment.NewLine
                             + "</style>");

                //expandable errors
                sw.WriteLine("<script type=\"text/javascript\">" + Environment.NewLine
                             + "function displ(ddd) {" + Environment.NewLine
                             + "if (document.getElementById(ddd).style.display == \'none\')" + Environment.NewLine
                             + "{document.getElementById(ddd).style.display = \'block\'}" + Environment.NewLine
                             + "else {document.getElementById(ddd).style.display = \'none\'}" + Environment.NewLine
                             + "}" + Environment.NewLine
                             + "</script>" + Environment.NewLine);
                sw.WriteLine("</head>");
                sw.WriteLine("<body>");
                sw.Close();
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
        private void WriteToConsole(string ms)
        {
            if (DuplicateToConsole)
                Console.WriteLine(ms);
        }
        public void LogDebug(string message)
        {
            lock (locker)
            {
                if (_logLevel>=LogLevel.Debug)
                {
                    WriteToFile(HtmlLogLine(message, "debug"));
                    WriteToConsole(message);
                }
            }
        }
        
        public void LogInfo(string message)
        {
            lock (locker)
            {
                if (_logLevel >= LogLevel.Info)
                {
                    WriteToFile(HtmlLogLine(message, "info"));
                    WriteToConsole(message);
                }
            }
        }

        public void LogWarning(Exception ex)
        {
            lock (locker)
            {
                if (_logLevel >= LogLevel.Warning)
                {
                    WriteToFile(HtmlLogException(ex, "warning"));
                    WriteToConsole(ex.ToString());
                }
            }
        }
        public void LogWarning(string message)
        {
            lock (locker)
            {
                if (_logLevel >= LogLevel.Warning)
                {
                    WriteToFile(HtmlLogLine(message,"warning"));
                    WriteToConsole(message);
                }
            }
        }

        public void LogError(Exception ex)
        {
            lock (locker)
            {
                if (_logLevel >= LogLevel.Error)
                {
                    WriteToFile(HtmlLogException(ex, "error"));
                    WriteToConsole(ex.Message);
                }
            }
        }
        public void LogError(string message)
        {
            lock (locker)
            {
                if (_logLevel >= LogLevel.Error)
                {
                    WriteToFile(HtmlLogLine(message, "error"));
                    WriteToConsole(message);
                }
            }
        }

        private void WriteToFile(string message)
        {
            WriteToFile(message,_workFilePath);
        }

        private void WriteToFile(string message, string path)
        {
            try
            {
                sw = new StreamWriter(path, true);
                sw.Write(message);
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _logLevel = LogLevel.Silent;
            }
        }
        private string HtmlLogLine(string message, string spanType)
        {
            var result = new StringBuilder();
            result.Append("<p> ");
            result.Append($"{TimeStamp + ":",-30}");
            result.Append("<span class=\"" +spanType + "\">"
                    + message
                    + "</span>");
            result.Append("<br>");
            result.Append(Environment.NewLine);
            return result.ToString();
        }
        private string HtmlLogException(Exception ex, string spanType)
        {
            var result = new StringBuilder();
            result.Append("<p> ");
            result.Append($"{TimeStamp + ":"}");
            
            result.Append("<span class=\"" + spanType + "\">");
            result.Append(ex.GetType()+":");
            result.Append(ex.Message);
            result.Append("</span>");

            if (ex.StackTrace != "")
            {
                result.Append("<a href=\"javascript: displ(\'" + _errorStackId + "\')\">Exception source...</a><br>" +
                              Environment.NewLine);
                result.Append("<div id=\"" + _errorStackId + "\" style=\"display: none;\">" + Environment.NewLine);
                _errorStackId++;
                result.Append("<span class=\"stacktrace\">");
                result.Append(ex.StackTrace);
                result.Append("</span>");
                result.Append("<a href=\"javascript: displ(\'" + _errorStackId + "\')\">Close</a></div>" +
                              Environment.NewLine);
            }

            if (ex.InnerException != null)
            {
                result.Append("<br>");
                result.Append(Environment.NewLine);
                result.Append("<span class=\"stacktrace\">");
                result.Append(ex.StackTrace);
                result.Append("</span>");
                result.Append(HtmlLogException(ex.InnerException, spanType));
            }
            result.Append("<br>");
            result.Append(Environment.NewLine);
            return result.ToString();
        }
        public void LogCrashDump(string sender, string version, IEnumerable<LogEntry> entries)
        {
            var path = CreateNewFile(CrashPreffix,_workDir);
            var dump = new StringBuilder();
            dump.Append(HtmlLogLine(sender+" ver. "+version+ " CRASH DUMP","info"));
            foreach (var entry in entries)
            {
                switch (entry.Level)
                {
                    case LogLevel.Debug:
                        dump.Append(HtmlLogLine(entry.Entry, "debug"));
                        break;
                    case LogLevel.Info:
                        dump.Append(HtmlLogLine(entry.Entry, "info"));
                        break;
                    case LogLevel.Warning:
                        if (!String.IsNullOrEmpty(entry.Entry))
                            dump.Append(HtmlLogLine(entry.Entry, "warning"));
                        if (entry.Exception != null)
                            dump.Append(HtmlLogException(entry.Exception, "warning"));
                        break;
                    case LogLevel.Error:
                        if (!String.IsNullOrEmpty(entry.Entry))
                            dump.Append(HtmlLogLine(entry.Entry, "error"));
                        if (entry.Exception != null)
                            dump.Append(HtmlLogException(entry.Exception, "error"));
                        break;
                }
            }
            dump.Append("</body>");
            dump.Append("</html>");
            WriteToFile(dump.ToString(),path);
        }
    }

   
}
