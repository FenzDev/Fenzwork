using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Services
{
    class ConstantsHelper
    {
        public static void SetGameName(string longName, string shortName)
        {
            FenzworkGame.LongName = longName;
            FenzworkGame.ShortName = shortName;
        }
        public static void AddAssetsWorkingDir(string assetsPath)
        {
            ((List<string>)Assets.DebugWorkingDirectories).Add(assetsPath);
        }
    }
}