using System;
using System.Collections.Generic;
using System.IO;
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
        public static void Run(GameCore core)
        {
            LongName = ConstantsHelper.GetAutoGenConstantByName("GameName").ToString();
            ShortName = ConstantsHelper.GetAutoGenConstantByName("GameShortName").ToString();

            
            //_AssetsPaths.Add(Assets.MainAssetsPath);
            
            using MGGame mg = new(core);

            mg.Run();
        }

    }
}
