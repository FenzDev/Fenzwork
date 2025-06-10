using Fenzwork.AssetsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Services
{
    public class AutoGenHelper
    {
        public static void SetGameInfo(string longName, string shortName)
        {
            FenzworkGame.LongName = longName;
            FenzworkGame.ShortName = shortName;
        }
        public static void AddDebugAssetsInfo(string configPath, string assetsPath)
        {
            if (assetsPath == string.Empty)
                return;

            ((List<(string, string)>)AssetsManager.DebugWorkingDirectories).Add((configPath, assetsPath));
        }
        public static void RegisterAsset(string assetInfoStr)
        {
            AssetsManager.RegisterAsset(new AssetInfo(assetInfoStr));
        }
    }
}