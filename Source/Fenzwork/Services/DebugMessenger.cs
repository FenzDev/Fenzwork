using Fenzwork.Services.MMF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Services
{
    public static class DebugMessenger
    {
        public static Process? DebuggingToolsProcess;
        static MessengerMMF MMF;

        public static readonly ConcurrentDictionary<Assembly, bool> _debugCache =
            new ConcurrentDictionary<Assembly, bool>();
        public static bool IsDebugBuild(Assembly asm) =>
            _debugCache.GetOrAdd(asm, a => {
                var dbg = a.GetCustomAttribute<DebuggableAttribute>();
                return dbg?.IsJITTrackingEnabled == true;
            });

        internal static void Init()
        {
            var callerAsm = Assembly.GetCallingAssembly();
            if (!IsDebugBuild(callerAsm))
                return;

            MMF = new MessengerMMF(FenzworkGame.LongName, FenzworkGame.ShortName, Process.GetCurrentProcess().StartTime.Ticks);
            MMF.Init();

            TryCleanOtherMessengers();
            if (Debugger.IsAttached)
                EnsureProcessRunning("Fenzwork.DebuggingTools", DateTime.Now.Ticks.ToString());

        }

        internal static void TryCleanOtherMessengers()
        {
            var files = Directory.GetFiles(Path.GetDirectoryName(MMF.FilePath));

            foreach (var file in files)
            {
                if (file == MMF.FilePath)
                    continue;

                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Do nothing
                }

            }
        }

        internal static void Log(sbyte type, DateTime dateTime, string message)
        {
            var callerAsm = Assembly.GetCallingAssembly();
            if (!IsDebugBuild(callerAsm))
                return;

            MMF.PendingMessages.Enqueue((type, dateTime, message));
        }

        internal static void Tick()
        {
            var callerAsm = Assembly.GetCallingAssembly();
            if (!IsDebugBuild(callerAsm))
                return;

            MMF.Tick();
        }

        internal static void Dispose()
        {
            var callerAsm = Assembly.GetCallingAssembly();
            if (!IsDebugBuild(callerAsm))
                return;

            MMF.Dispose();
        }

        static void EnsureProcessRunning(string baseName, string args)
        {
            // 1. Try to get the full path; if not found, just return (ignore).
            string? exePath = GetExecutablePathInSameFolder(baseName);
            if (exePath == null)
                return;

            // 2. If it’s not already running, start it.
            if (!IsProcessAlreadyRunning(baseName, exePath))
                DebuggingToolsProcess = StartProcess(exePath, args);
        }

        static string? GetExecutablePathInSameFolder(string baseName)
        {
            string fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? baseName + ".exe"
                : baseName;

            string fullPath = Path.Combine(AppContext.BaseDirectory, fileName);
            return File.Exists(fullPath) ? fullPath : null;
        }

        static bool IsProcessAlreadyRunning(string baseName, string exeFullPath)
        {
            string target = Path.GetFullPath(exeFullPath);
            return Process.GetProcessesByName(baseName).Any(p =>
            {
                try
                {
                    var path = Path.GetFullPath(p.MainModule.FileName);
                    
                    var result = string.Equals(
                        Path.GetFullPath(p.MainModule.FileName),
                        target,
                        StringComparison.OrdinalIgnoreCase
                    );

                    
                    return result;
                }
                catch
                {
                    return false;
                }
            });
        }

        static Process StartProcess(string exePath, string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(exePath) ?? "",
            };
            return Process.Start(psi);
        }
    }
}
