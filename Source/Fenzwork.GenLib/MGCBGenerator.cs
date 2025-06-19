using Fenzwork.GenLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fenzwork.GenLib
{
    public class MGCBGenerator
    {
        public static MainConfig Main;
        public static StreamWriter Writer;

        internal static void WriteHeader()
        {
// outputDir:bin /$(Platform)
// intermediateDir:obj /$(Platform)
// platform:DesktopGL
// config:
// profile:Reach
// compress:False
            Writer.WriteLine($"# --- This file is auto generated, any modification will be gone once regenerated --- #");
            Writer.WriteLine($"/outputDir:bin/$(Platform)");
            Writer.WriteLine($"/intermediateDir:obj/$(Platform)");
            Writer.WriteLine($"/platform:{Main.BuildPlatform}");
            Writer.WriteLine($"/compress:{Main.BuildCompress}");
            Writer.WriteLine($"/profile:{Main.BuildProfile}");
            foreach ( var reference in Main.BuildReferences ) 
                Writer.WriteLine($"/reference:{reference}");
            Writer.WriteLine();
        }
        internal static void WriteAsset( AssetsGroupConfig config, string assetName)
        {

            if (config.Method.Equals("build", StringComparison.OrdinalIgnoreCase))
            {
                WriteBuildAsset(config, assetName);
            }
            else if (config.Method.Equals("copy", StringComparison.OrdinalIgnoreCase))
            {
                WriteCopyAsset(config, assetName);
            }
            else
                throw new Exception($"Unknown method with the name {config.Method}.");
        }

        static void WriteBuildAsset(AssetsGroupConfig config, string assetName)
        {
            if (config.BuildImporter == "")
                throw new Exception("Build importer is empty or not specified");
            if (config.BuildProcessor == "")
                throw new Exception("Build processor is empty or not specified");

            Writer.WriteLine($"#begin {assetName}");
            Writer.WriteLine($"/importer:{config.BuildImporter}");
            Writer.WriteLine($"/processor:{config.BuildProcessor}");
            foreach (var processorParam in config.BuildProcessorParams)
            {
                Writer.WriteLine($"/processorParam:{processorParam}");
            }
            Writer.WriteLine($"/build:{assetName}");
            Writer.WriteLine();

        }
        static void WriteCopyAsset(AssetsGroupConfig config, string assetName)
        {
            Writer.WriteLine($"#begin {assetName}");
            Writer.WriteLine($"/copy:{assetName}");
            Writer.WriteLine();
        }
    }
}
