using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fenzwork.DebuggingTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (SimilarProcessExistsAlready())
            {
                Console.WriteLine("Error : Process like this is working already.\nThis process will then be terminated.");
                Environment.Exit(1);
                return; 
            }
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            MessagingManager.Start(args);
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e) => MessagingManager.Dispose();

        static bool SimilarProcessExistsAlready()
        {
            var current = Process.GetCurrentProcess();
            var cloneProcesses = Process.GetProcessesByName(current.ProcessName);

            foreach (var process in cloneProcesses)
            {
                if (process.Id == current.Id)
                    continue;

                if (process.MainModule == null)
                    return false;

                if (process.MainModule.FileName == current.MainModule!.FileName)
                    return true;
                    
            }

            return false;
        }
    
    }
}
