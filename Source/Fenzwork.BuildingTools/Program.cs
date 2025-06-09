using System.Diagnostics;

namespace Fenzwork.BuildingTools
{
    internal class Program
    {

        // When called with arguments it generates .mgcb file
        // (arg0) -> Config file location
        // (arg1) -> Assets working dir
        // (arg1) -> MGCB file output location
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            Console.WriteLine(string.Join('\n', args));

            if (args.Length != 4)
            {
                Console.WriteLine("Wrong argumments.");
                Console.WriteLine("<ConfigFile> <ProjectsAssetsPath> <AssetsWorkingDir> <MGCBOutputFile>");
                return;
            }

            MGCBGenerator.ConfigPath = args[0];
            MGCBGenerator.ProjectAssetsPath = args[1];
            MGCBGenerator.AssetsFullPath = args[2];
            MGCBGenerator.MGCBPath = args[3];

            MGCBGenerator.Generate();
        }
    }
}
