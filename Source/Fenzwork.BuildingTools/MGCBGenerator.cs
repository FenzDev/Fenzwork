using Fenzwork.AssetsLibrary;
using Fenzwork.AssetsLibrary.Models;
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
            Console.WriteLine("Starting MGCB Generator");

            var configFile = File.OpenRead(ConfigPath);
            if (configFile == null)
            {
                Console.WriteLine($"Couldn't open config file ({ConfigPath})");
                return;
            }

            Directory.CreateDirectory(AssetsPath);
            var mgcbFileWriter = File.CreateText(MGCBPath);
            if (mgcbFileWriter == null)
            {
                Console.WriteLine($"Couldn't create/overwrite mgcb file ({MGCBPath})");
                return;
            }

            var globalObjDir = Path.Combine(AssetsPath, "obj", ".global");
            Directory.CreateDirectory(globalObjDir);
            var assetsCatalogWriter = File.CreateText(Path.Combine(globalObjDir, "assets_catalog.cache"));

            var assetsConfig = JsonSerializer.Deserialize<AssetsConfig>(configFile, new JsonSerializerOptions() { AllowTrailingCommas=true, ReadCommentHandling=JsonCommentHandling.Skip});
            if (assetsConfig == null)
            {
                Console.WriteLine($"Couldn't understand the config file ({ConfigPath})\nMake sure you follow the correct pattern.");
                return;
            }

            MGCBWriteHeader(mgcbFileWriter, assetsConfig);

            var assetsDirWrapper = new DirectoryInfoWrapper(Directory.CreateDirectory(AssetsPath));
            var excludeSet = new HashSet<string>(["**/bin/**", "**/obj/**"]);

            foreach (var config in assetsConfig.Configurations)
            {
                Utilities.DealWithAssetFiles((cfg, files) => {
                    
                    MGCBWriteForFiles(mgcbFileWriter, cfg, files);
                    if (!cfg.Type.Equals("ignore", StringComparison.OrdinalIgnoreCase)) AssetsCatalogWriteFiles(assetsCatalogWriter, files);

                }, assetsDirWrapper, config, ref excludeSet); 
            }

            assetsCatalogWriter.Close();
            configFile.Close();
            mgcbFileWriter.Close();
        }

        static void AssetsCatalogWriteFiles(StreamWriter writer, IEnumerable<FilePatternMatch> files)
        {
            foreach (var file in files)
            {
                writer.WriteLine(file.Path);
            }
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

        static bool MGCBWriteForFiles(StreamWriter writer, Configuration config, IEnumerable<FilePatternMatch> files)
        {
            if (config.Type.Equals("build", StringComparison.OrdinalIgnoreCase))
                MGCBWriteBuild(writer, config, files);
            else if (config.Type.Equals("copy", StringComparison.OrdinalIgnoreCase))
                MGCBWriteCopy(writer, config, files);
            else
            {
                throw new Exception($"Unknown {config.Type} type specified in config with patterns ({string.Join(',', config.Include)}).");
            
            }

            writer.WriteLine();
            return true;
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
