using Fenzwork.AssetsLibrary.Models;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fenzwork.AssetsLibrary
{
    public static class Utilities
    {
        public static List<(Configuration Config, string File)> DeconstructAssetsFolder(AssetsConfig assetsConfig, string directory)
        {
            var list = new List<(Configuration Config, string File)>();

            var excluded = new HashSet<string>(["**/bin/**", "**/obj/**"]);
            foreach (var configuration in assetsConfig.Configurations)
                DeconstructAssetsByConfiguration(configuration, ref list, directory, ref excluded);

            return list;
        }
        
        public static void DeconstructAssetsByConfiguration(Configuration config, ref List<(Configuration Config, string File)> list, string directory, ref HashSet<string> excluded)
        {
            foreach (var exception in config.Exceptions)
                DeconstructAssetsByConfiguration(exception, ref list, directory, ref excluded);

            var matcher = new Matcher(StringComparison.Ordinal);

            matcher.AddExcludePatterns(excluded);
            matcher.AddIncludePatterns(config.Include);

            foreach (var included in config.Include)
                excluded.Add(included);

            foreach (var file in matcher.GetResultsInFullPath(directory))
                list.Add((config, file));
        }

        public static void DealWithAssetFiles(Action<Configuration, IEnumerable<FilePatternMatch>> fileAction, DirectoryInfoWrapper assetsDirWrapper, Configuration config, ref HashSet<string> excluded)
        {
            foreach (var exception in config.Exceptions)
            {
                DealWithAssetFiles(fileAction, assetsDirWrapper, exception, ref excluded);
            }

            var matcher = new Matcher(StringComparison.Ordinal);
            matcher.AddExcludePatterns(excluded);

            foreach (var includePattern in config.Include)
            {
                matcher.AddInclude(includePattern);
                excluded.Add(includePattern);
            }

            if (config.Type.Equals("ignore", StringComparison.OrdinalIgnoreCase))
                return;

            fileAction(config, matcher.Execute(assetsDirWrapper).Files);
        }
    }
}
