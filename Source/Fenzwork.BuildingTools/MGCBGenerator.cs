using Fenzwork.AssetsLibrary;
using Fenzwork.AssetsLibrary.Models;
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
        public static string ProjectAssetsPath;
        public static string AssetsFullPath;
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

            Directory.CreateDirectory(AssetsFullPath);
            var mgcbFileWriter = File.CreateText(MGCBPath);
            if (mgcbFileWriter == null)
            {
                Console.WriteLine($"Couldn't create/overwrite mgcb file ({MGCBPath})");
                return;
            }

            var globalObjDir = Path.Combine(AssetsFullPath, "obj", ".global");
            Directory.CreateDirectory(globalObjDir);
            var assetsCatalogWriter = File.CreateText(Path.Combine(globalObjDir, "assets_catalog.cache"));

            var assetsConfig = JsonSerializer.Deserialize<MainConfig>(configFile, new JsonSerializerOptions() { AllowTrailingCommas=true, ReadCommentHandling=JsonCommentHandling.Skip});
            if (assetsConfig == null)
            {
                Console.WriteLine($"Couldn't understand the config file ({ConfigPath})\nMake sure you follow the correct pattern.");
                return;
            }

            MGCBWriteHeader(mgcbFileWriter, assetsConfig);

            var excludeSet = new HashSet<string>(["**/bin/**", "**/obj/**"]);

            Utilities.DealWithAssets(assetsConfig, (elm,config) => {

                MGCBWriteForFiles(mgcbFileWriter, config, elm);
                AssetsCatalogWriteFile(assetsCatalogWriter, elm);

            }, ProjectAssetsPath, AssetsFullPath);

            assetsCatalogWriter.Close();
            configFile.Close();
            mgcbFileWriter.Close();
        }

        static void AssetsCatalogWriteFile(StreamWriter writer, AssetInfo assetInfo)
        {
            writer.WriteLine(assetInfo.ToString());
        }

        static void MGCBWriteHeader(StreamWriter writer, MainConfig assetsConfig)
        {
            writer.WriteLine($"# --- Auto-Generated MonoGame Content Reference File --- #");
            //writer.WriteLine($"/w:{AssetsFullPath}");
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

        static bool MGCBWriteForFiles(StreamWriter writer, GroupConfig config, AssetInfo assetInfo)
        {
            if (assetInfo.Method == "build")
                MGCBWriteBuild(writer, config, assetInfo);
            else if (config.Method.Equals("copy", StringComparison.OrdinalIgnoreCase))
                MGCBWriteCopy(writer, assetInfo);
            else
            {
                throw new Exception($"Unknown {assetInfo.Method} type specified in config with patterns ({string.Join(',', config.Include)}).");
            }

            writer.WriteLine();
            return true;
        }

        static void MGCBWriteBuild(StreamWriter writer, GroupConfig config, AssetInfo assetInfo)
        {
            writer.Write("#begin ");
            writer.WriteLine(assetInfo.AssetPath);
            foreach (var property in config.Properties)
            {
                writer.Write('/');
                writer.WriteLine(property);
            }
            writer.Write("/build:");
            writer.WriteLine(assetInfo.AssetPath);
        }

        static void MGCBWriteCopy(StreamWriter writer, AssetInfo assetInfo)
        {
            writer.Write("#begin ");
            writer.WriteLine(assetInfo.AssetPath);
            writer.Write("/copy:");
            writer.WriteLine(assetInfo.AssetPath);
        }

    }
}
