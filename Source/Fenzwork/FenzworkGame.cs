using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fenzwork.Services;

namespace Fenzwork
{
    public static class FenzworkGame
    {
        public static string LongName { get; private set; }
        public static string ShortName { get; private set; }

        public static List<string> _AssetsPaths { get; set; } = new();
        public static void Run(string gameName, string gameShortName, GameCore core)
        {
            LongName = gameName;
            ShortName = gameShortName;

            _AssetsPaths.Add(Assets.MainAssetsPath);
            
            using MGGame mg = new(core);

            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                _AssetsPaths.AddRange(Environment.GetCommandLineArgs());
            }

            mg.Run();
        }

    }
}
