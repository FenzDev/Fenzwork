using Fenzwork.GenLib.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.GenLib
{
    public static class Utilities
    {
        public static ConfigMatchResult? FileMatchesConfig(string assetsDir, string filePath, MainConfig config )
        {
            ConfigMatchResult? result = null;
            GoThroughConfig(config, assetsDir, (localDir, groupConfig, files) =>
            {
                var arry = files.ToArray();
                if (arry.Contains(filePath))
                {
                    result = new ConfigMatchResult(Path.GetRelativePath(assetsDir, filePath).Replace('\\', '/'), groupConfig);
                    return false;
                }
                return true;
            });
            return result;
        }

        public static void GoThroughConfig(MainConfig mainConfig, string assetsDirPath, Func<string, AssetsGroupConfig, IEnumerable<string>, bool> doContinue)
        {
            if (mainConfig.EnableDomainFolders)
                foreach (var assetGroupConfig in mainConfig.Assets)
                {
                    assetGroupConfig.From = assetGroupConfig.From.TrimEnd('/', '\\');
                    assetGroupConfig.Method = assetGroupConfig.Method.ToLower();

                    foreach (var dir in Directory.EnumerateDirectories(assetsDirPath))
                    {
                        if (assetGroupConfig.Method.Equals("ignore"))
                            continue;

                        if (!GoThroughDirectory(Path.Combine(dir, assetGroupConfig.From), false, assetGroupConfig, doContinue))
                            return;
                    }
                }
            else
            {

                foreach (var assetGroupConfig in mainConfig.Assets)
                {
                    assetGroupConfig.From = assetGroupConfig.From.TrimEnd('/', '\\');
                    assetGroupConfig.Method = assetGroupConfig.Method.ToLower();

                    if (assetGroupConfig.Method.Equals("ignore"))
                        continue;

                    if (!GoThroughDirectory(Path.Combine(assetsDirPath, assetGroupConfig.From), string.IsNullOrEmpty(assetGroupConfig.From), assetGroupConfig, doContinue))
                        return;
                }

            }
        }
        static bool GoThroughDirectory(string localDir, bool isRootAssetDir, AssetsGroupConfig groupConfig, Func<string, AssetsGroupConfig, IEnumerable<string>, bool> doContinue)
        {
            if (!Directory.Exists(localDir))
                return true;
            
            return doContinue(localDir, groupConfig, groupConfig.Include.SelectMany(
                pattern => Directory.EnumerateFiles(localDir, pattern, SearchOption.AllDirectories))
                .Where(path => !isRootAssetDir || !StringStartWithAny(Path.GetRelativePath(localDir, path), $"bin{Path.DirectorySeparatorChar}", $"obj{Path.DirectorySeparatorChar}")));
        }

        static bool StringStartWithAny(string str, params string[] withOr)
        {
            for (int i = 0; i < withOr.Length; i++)
            {
                if (str.StartsWith(withOr[i]))
                    return true;
            }
            return false;
        }
    }

    public record struct ConfigMatchResult(string AssetName, AssetsGroupConfig GroupConfig);
}
