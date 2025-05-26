using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fenzwork.DebuggingTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (ProcessExistsAlready())
            {
                Console.WriteLine("Error : Process like this is working already.\nThis process will then be terminated.");
                Environment.Exit(1);
                return;
            }

            MessagingManager.Start(args);
        }

        static bool ProcessExistsAlready()
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
