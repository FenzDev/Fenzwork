using Fenzwork.AssetsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fenzwork.AssetsLibrary
{
    public static class Utilities
    {
        public static void DealWithAssets(MainConfig mainConfig, Action<AssetInfo, GroupConfig>? assetAction, string assetsLocation, string directory)
        {            
            var excluded = new HashSet<string>([""]);

            if (!mainConfig.EnableDomains)
            {
                DealWithAssetsBased(mainConfig, assetAction, assetsLocation, directory, directory);
                return;
            }

            foreach (var domainDir in Directory.GetDirectories(directory))
            {
                DealWithAssetsBased(mainConfig, assetAction, assetsLocation, directory, domainDir);
            }

        }
        static void DealWithAssetsBased(MainConfig mainConfig, Action<AssetInfo, GroupConfig>? assetAction, string assetsLocation, string assetsDirectory, string domainDirectory)
        {
            //Debugger.Launch();
            foreach (var config in mainConfig.Configurations)
            {
                var method = config.Method.ToLower();
                if (method == "ignore")
                    continue;

                var basedDirectory = Path.Combine(domainDirectory, config.BaseFolder);
                if (!Directory.Exists(basedDirectory))
                    continue;

                var files = config.Include.SelectMany(pattern => Directory.GetFiles(basedDirectory, pattern));

                foreach (var file in files)
                {
                    var assetPath = Path.GetRelativePath(assetsDirectory, file);
                    if (!TryDecomposeAssetNameAndParameter(assetPath, out var assetName, out var parameter))
                        continue;

                    var assetInfo = new AssetInfo()
                    {
                        Method = config.Method,
                        Type = config.Type,
                        AssetName = assetName,
                        Domain = mainConfig.EnableDomains? Path.GetRelativePath(assetsDirectory, domainDirectory): "",
                        Parameter = parameter,
                        AssetPath = assetPath,
                        AssetsPath = assetsLocation
                    };

                    if (assetAction != null)
                        assetAction.Invoke(assetInfo, config);
                }
                
            }
        }

        public static bool TryDecomposeAssetNameAndParameter(string name, out string assetName, out string parameter)
        {
            // Normalize to system separators
            assetName = name.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Split using both separator types
            string[] parts = assetName.Split(new[] {
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar
                    }, StringSplitOptions.RemoveEmptyEntries);

            // if directory starts with '.' or a digit it will be ignored
            if (parts.Any(s => s.StartsWith('.')))
            {
                parameter = "";
                assetName = "";
                return false;
            }

            // Remove trailing extensions
            parts[parts.Length -1] = parts[parts.Length - 1].Split('.')[0];

            var fileNameSplits = parts[parts.Length - 1].Split('[');
            parameter = fileNameSplits.Length > 1? string.Join(',', fileNameSplits[1..]!.Select(s => s.Substring(0, s.Length - 1))): "";

            parts[parts.Length - 1] = fileNameSplits[0];

            // Join using the custom splitter
            assetName = string.Join('.', parts);
            return true;
        }
    }
}
