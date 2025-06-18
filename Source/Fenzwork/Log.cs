using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork
{
    public static class Log
    {

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Debug(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            var fileName = Path.GetFileName(file);

            FlushMessage(LogType.Debug, $"[{fileName}/L:{line}] {message}");
        }

        public static void Info(string message)
        {
            FlushMessage(LogType.Info, $" {message}");
        }

        public static void Warning(string message)
        {
            FlushMessage(LogType.Warning, $" {message}");
        }

        public static void Error(string message)
        {
            FlushMessage(LogType.Error, $" {message}");
        }


        static void FlushMessage(LogType type, string message)
        {
            DateTime time = DateTime.Now;
            switch (type)
            {
                case LogType.Debug:
                    Debugger.Log(2, "dbg", GenerateMessage(time, "dbg", message));
                    break;
                case LogType.Info:
                    Debugger.Log(3, "info", GenerateMessage(time, "info", message));
                    break;
                case LogType.Warning:
                    Debugger.Log(2, "warn", GenerateMessage(time, "warn", message));
                    break;
                case LogType.Error:
                    Debugger.Log(1, "error", GenerateMessage(time, "error", message));
                    break;
            }
        }

        static string GenerateMessage(DateTime time, string type, string message)
        {
            return $"[{time:HH:mm:ss.ff}][{type}]{message}\n";
        }

        public enum LogType
        {
            Debug,
            Info,
            Warning,
            Error,
        }
    }
}
