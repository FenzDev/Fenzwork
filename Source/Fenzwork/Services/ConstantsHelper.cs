using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Services
{
    public class ConstantsHelper
    {
        public static void SetGameName(string longName, string shortName)
        {
            FenzworkGame.LongName = longName;
            FenzworkGame.ShortName = shortName;
        }
        public static void AddAssets(string configPath, string assetsPath)
        {
            if (assetsPath == string.Empty)
                return;

            ((List<string>)Assets.DebugWorkingDirectories).Add(assetsPath);
        }
    }
}