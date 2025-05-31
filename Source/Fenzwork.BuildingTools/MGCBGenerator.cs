using Fenzwork.BuildingTools.Models;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fenzwork.BuildingTools
{
    internal static class MGCBGenerator
    {
        public static string AssetsPath;
        public static string ConfigPath;
        public static string MGCBPath;

        public static void Generate()
        {

            var configFile = File.OpenRead(ConfigPath);
            if (configFile == null)
            {
                Console.WriteLine($"Couldn't open config file ({ConfigPath})");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(MGCBPath));
            var mgcbFileWriter = File.CreateText(MGCBPath);
            if (mgcbFileWriter == null)
            {
                Console.WriteLine($"Couldn't create/overwrite mgcb file ({MGCBPath})");
                return;
            }

            var assetsConfig = JsonSerializer.Deserialize<AssetsConfig>(configFile, new JsonSerializerOptions() { AllowTrailingCommas=true, ReadCommentHandling=JsonCommentHandling.Skip});
            if (assetsConfig == null)
            {
                Console.WriteLine($"Couldn't understand the config file ({ConfigPath})\nMake sure you follow the pattern.");
                return;
            }

            MGCBWriteHeader(mgcbFileWriter, assetsConfig);

            var assetsDirWrapper = new DirectoryInfoWrapper(Directory.CreateDirectory(AssetsPath));
            var excludeSet = new HashSet<string>();

            foreach (var config in assetsConfig.Configurations)
            {
                MGCBWriteBasedOnConfiguration(mgcbFileWriter, assetsDirWrapper, config, ref excludeSet);
            }

            configFile.Close();
            mgcbFileWriter.Close();
        }

        static void MGCBWriteBasedOnConfiguration(StreamWriter writer, DirectoryInfoWrapper assetsDirWrapper, Configuration config, ref HashSet<string> excluded)
        {
            foreach (var exception in config.Exceptions)
            {
                MGCBWriteBasedOnConfiguration(writer, assetsDirWrapper, exception, ref excluded);
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

            MGCBWriteForFiles(writer, config, matcher.Execute(assetsDirWrapper).Files );
        }

        static void MGCBWriteHeader(StreamWriter writer, AssetsConfig assetsConfig)
        {
            writer.WriteLine($"# --- Auto-Generated MonoGame Content Reference File --- #");
            //writer.WriteLine($"/w:{AssetsPath}");
            writer.WriteLine($"/outputDir:bin/$(Platform)");
            writer.WriteLine($"/intermediateDir:obj/$(Platform)");
            writer.WriteLine($"/platform:DesktopGL");
            writer.WriteLine($"/config:");
            writer.WriteLine($"/profile:{assetsConfig.Profile}");
            writer.WriteLine($"/compress:{assetsConfig.Compress}");
            writer.WriteLine();
            foreach (var reference in assetsConfig.References)
            {
                writer.WriteLine($"/reference:../{assetsConfig.Compress}");
            }
            writer.WriteLine();
        }

        static void MGCBWriteForFiles(StreamWriter writer, Configuration config, IEnumerable<FilePatternMatch> files)
        {
            if (config.Type.Equals("build", StringComparison.OrdinalIgnoreCase))
            {
                MGCBWriteBuild(writer, config, files);
            }
            else if (config.Type.Equals("copy", StringComparison.OrdinalIgnoreCase))
            {
                MGCBWriteCopy(writer, config, files);
            }
            else
                Console.WriteLine($"Unknown {config.Type} type specified in config with patterns ({string.Join(',', config.Include)}).\nThus we will skip them.");

            writer.WriteLine();
        }

        static void MGCBWriteBuild(StreamWriter writer, Configuration config, IEnumerable<FilePatternMatch> files)
        {
            if (files.Count() == 0)
                return;

            foreach (var property in config.Properties)
            {
                writer.Write('/');
                writer.WriteLine(property);
            }

            foreach (var file in files)
            {
                writer.Write("#begin ");
                writer.WriteLine(file.Path);
                writer.Write("/build:");
                writer.WriteLine(file.Path);

            }
        }

        static void MGCBWriteCopy(StreamWriter writer, Configuration config, IEnumerable<FilePatternMatch> files)
        {
            foreach (var file in files)
            {
                writer.Write("#begin ");
                writer.WriteLine(file.Path);
                writer.Write("/copy:");
                writer.WriteLine(file.Path);
            }
        }

    }
}
