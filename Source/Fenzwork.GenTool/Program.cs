
using Fenzwork.GenLib;
using System.Diagnostics;
using System.Reflection;

namespace Fenzwork.GenTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 6)
            {
                Console.WriteLine("[Fenzwork.GenTool] [Error] Number of arguments are missing.");
                Console.WriteLine(string.Join("\n\t", args));
                Console.WriteLine("[Fenzwork.GenTool] [Error] Correct arguments format: <AssetsConfigFile> <AssetsBaseDir> <IntermidateDir> <MGCBFileName> <Configuration>");
                Environment.Exit(1);
            }

            GenManager.AssetsConfigFile = args[0];
            GenManager.AssetsBaseDir = args[1];
            GenManager.IntermidateDir = args[2]; 
            GenManager.Namespace = args[3]; 
            GenManager.MGCBFileName = args[4];
            GenManager.IsDebug = args[5].Equals("Debug", StringComparison.OrdinalIgnoreCase);

            try
            {
                GenManager.Start();
            }
            catch (Exception) { Debugger.Launch(); }
        }
    }
}
