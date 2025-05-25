using Fenzwork.Services.MMF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Services
{
    public static class DebugMessenger
    {
        public static Process? DebuggingToolsProcess;
        static MessengerMMF MMF;

        internal static void Init()
        {
            if (Debugger.IsAttached)
            {
                EnsureProcessRunning("Fenzwork.DebuggingTools");

            }

            MMF = new MessengerMMF(FenzworkGame.LongName, FenzworkGame.ShortName, Process.GetCurrentProcess().StartTime.Ticks);
            MMF.Init();

        }

        internal static void Log(sbyte type, string message)
        {
            MMF.PendingMessages.Enqueue((type,message));
        }

        internal static void Tick()
        {

            MMF.Tick();
        }

        static void EnsureProcessRunning(string baseName)
        {
            // 1. Try to get the full path; if not found, just return (ignore).
            string? exePath = GetExecutablePathInSameFolder(baseName);
            if (exePath == null)
                return;

            // 2. If it’s not already running, start it.
            if (!IsProcessAlreadyRunning(baseName, exePath))
                DebuggingToolsProcess = StartProcess(exePath);
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

        static Process StartProcess(string exePath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(exePath) ?? "",
            };
            return Process.Start(psi);
        }
    }
}
