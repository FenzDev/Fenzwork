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
        static readonly ConcurrentDictionary<Assembly, bool> _debugCache =
            new ConcurrentDictionary<Assembly, bool>();
        static bool IsDebugBuild(Assembly asm) =>
            _debugCache.GetOrAdd(asm, a => {
                var dbg = a.GetCustomAttribute<DebuggableAttribute>();
                return dbg?.IsJITTrackingEnabled == true;
            });

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Debug(string message)
        {
            if (!Debugger.IsAttached)
                return;

            var callerAsm = Assembly.GetCallingAssembly();
            if (!IsDebugBuild(callerAsm))
                return;

            var frame = new StackTrace(true).GetFrame(1);
            var file = Path.GetFileName(frame.GetFileName());
            var line = frame.GetFileLineNumber();

            FlushMessage(LogType.Debug, $"[{file}/L:{line}] {message}");
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
                    Debugger.Log(2, "info", GenerateMessage(time, "info", message));
                    break;
                case LogType.Warning:
                    Debugger.Log(1, "warn", GenerateMessage(time, "warn", message));
                    break;
                case LogType.Error:
                    Debugger.Log(0, "error", GenerateMessage(time, "error", message));
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
