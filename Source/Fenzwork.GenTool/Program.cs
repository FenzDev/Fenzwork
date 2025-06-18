
using Fenzwork.GenLib;
using System.Diagnostics;
using System.Reflection;

namespace Fenzwork.GenTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine(string.Join("\n\t", args));
                    Console.WriteLine("[Error] Correct arguments format: <AssetsConfigFile> <AssetsBaseDir> <IntermidateDir> <MGCBFileName>");
                    Environment.Exit(1);

            }

            GenManager.AssetsConfigFile = args[0];
            GenManager.AssetsBaseDir = args[1];
            GenManager.IntermidateDir = args[2].Trim('"'); 
            GenManager.MGCBFileName = ".mgcref.cache"; // For some reasons it always gives me " at the end

            try
            {
                GenManager.Start();
            }
            catch (Exception ex) { Debugger.Launch(); }
        }
    }
}
