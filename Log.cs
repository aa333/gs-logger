using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace GSLogger
{
    public static class Log
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern MessageBoxResult MessageBox(IntPtr hWnd, String text, String caption, uint options);
        static readonly FixedSizeQueue<LogEntry> Crshdump;

        private static readonly object locker = new object();
        static readonly List<IDebugCrashDumper> CrashDumpListeners = new List<IDebugCrashDumper>();
        static readonly List<IDebugListener> SystemListeners = new List<IDebugListener>();
        private const int CrashDumpCacheLimit=60;
        private const uint MB_ICONERROR = 0x00000010;
        private const uint MB_SYSTEMMODAL = 0x00001000;

        public static bool DoCrashDump { get; set; }


        static Log()
        {
            Crshdump = new FixedSizeQueue<LogEntry>(CrashDumpCacheLimit);
            DoCrashDump = true;
        }
        
        public static void SubscribeToUnhadledExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
    
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex == null) return;
            Error("Unhandled exception in "+AppDomain.CurrentDomain, ex);
            LogCrashDump(Thread.CurrentThread.Name, "");
            var msgboxTxt = new StringBuilder();
            msgboxTxt.Append("Fatal error: ");
            msgboxTxt.Append(ex.GetType());
            msgboxTxt.Append(": "+ex.Message);
            msgboxTxt.Append(Environment.NewLine);
            msgboxTxt.Append("Details in the crashdump.");
            MessageBox(IntPtr.Zero, msgboxTxt.ToString(), "Error", MB_ICONERROR | MB_SYSTEMMODAL);
            Environment.Exit(ex.HResult);
        }
  

        public static void RegisterSystemEventListener(IDebugListener logger)
        {
            lock(locker)
                SystemListeners.Add(logger);
        }
        public static void RemoveSystemEventListener(IDebugListener logger)
        {
            lock (locker)
                SystemListeners.Remove(logger);
        }
        public static void RegisterCrashDumpListener(IDebugCrashDumper logger)
        {
            lock (locker)
                CrashDumpListeners.Add(logger);
        }
        public static void RemoveCrashDumpListener(IDebugCrashDumper logger)
        {
            lock (locker)
                CrashDumpListeners.Remove(logger);
        }

        public static void LogCrashDump(string sender, string version)
        {
            lock (locker)
            {
                if (DoCrashDump)
                {
                    var lastEntries = Crshdump.ToList();

                    foreach (var item in CrashDumpListeners)
                    {
                        item.LogCrashDump(sender, version, lastEntries);
                    }
                }
            }

        }

        public static void Error(string message)
        {
            lock (locker)
            {
                if (DoCrashDump)
                    Crshdump.Enqueue(new LogEntry(message, DateTime.Now, LogLevel.Error));
                foreach (var item in SystemListeners)
                {
                    item.LogError(message);
                }
            }
        }

        public static void Error(Exception ex)
        {
            lock (locker)
            {
                if (DoCrashDump)
                    Crshdump.Enqueue(new LogEntry(null, DateTime.Now, LogLevel.Error, ex));
                foreach (var item in SystemListeners)
                {
                    item.LogError(ex);
                }
            }
        }

        public static void Error(string message, Exception ex)
        {
            lock (locker)
            {
                if (DoCrashDump)
                    Crshdump.Enqueue(new LogEntry(message, DateTime.Now, LogLevel.Error, ex));
                foreach (var item in SystemListeners)
                {
                    item.LogError(message+Environment.NewLine+ex);
                }
            }
        }

        public static void Warning(string message)
        {
            lock (locker)
            {
                if (DoCrashDump)
                    Crshdump.Enqueue(new LogEntry(message, DateTime.Now, LogLevel.Warning));
                foreach (var item in SystemListeners)
                {
                    item.LogWarning(message);
                }
            }
        }

        public static void Warning(Exception ex)
        {
            lock (locker)
            {
                if (DoCrashDump)
                    Crshdump.Enqueue(new LogEntry(null, DateTime.Now, LogLevel.Warning, ex));
                foreach (var item in SystemListeners)
                {
                    item.LogWarning(ex);
                }
            }
        }

        public static void Warning(string message, Exception ex)
        {
            lock (locker)
            {
                if (DoCrashDump)
                    Crshdump.Enqueue(new LogEntry(message, DateTime.Now, LogLevel.Warning, ex));
                foreach (var item in SystemListeners)
                {
                    item.LogWarning(message+Environment.NewLine+ex);
                }
            }
        }

        public static void Info(string message)
        {
            lock (locker)
            {
                if (DoCrashDump)
                    Crshdump.Enqueue(new LogEntry(message, DateTime.Now, LogLevel.Info));
                foreach (var item in SystemListeners)
                {
                    item.LogInfo(message);
                }
            }
        }

        public static void Debug(string message)
        {
            lock (locker)
            {
                if (DoCrashDump)
                    Crshdump.Enqueue(new LogEntry(message, DateTime.Now, LogLevel.Debug));
                foreach (var item in SystemListeners)
                {
                    item.LogDebug(message);
                }
            }
        }
    }

    public enum MessageBoxResult : uint
    {
        Ok = 1,
        Cancel,
        Abort,
        Retry,
        Ignore,
        Yes,
        No,
        Close,
        Help,
        TryAgain,
        Continue,
        Timeout = 32000
    }
}
