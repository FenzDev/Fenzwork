using Fenzwork.GenLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.GenLib
{

    public class AssetsRegistryClassGenerator
    {
        public static StreamWriter Writer;

        public static void WriteHead(MainConfig mainConfig)
        {
            Writer.WriteLine($"// --- This file is auto generated, any modification will be gone once regenerated --- //");
            Writer.WriteLine("using Fenzwork;");
            Writer.WriteLine("using Fenzwork.Systems.Assets;");
            Writer.WriteLine($"namespace Fenzwork._AutoGen\n{{");
            Writer.WriteLine("\tinternal static class AssetsRegistry\n\t{");
            Writer.WriteLine($"\t\tprivate const bool IsDebug = {GenManager.IsDebug.ToString().ToLower()};");
            var workingDir = GenManager.IsDebug ? GenManager.AssetsDirectory.Replace("\\", "\\\\") : "";
            Writer.WriteLine($"\t\tprivate const string WorkingDirectory = \"{workingDir}\";");
            var configFile = GenManager.IsDebug ? GenManager.AssetsConfigFile.Replace("\\", "\\\\") : "";
            Writer.WriteLine($"\t\tprivate const string AssetsConfigFile = \"{configFile}\";");
            Writer.WriteLine($"\t\tprivate const string AssetsDirectoryName = \"{mainConfig.AssetsDirectoryName}\";");
            Writer.WriteLine("\t\tprivate static void Register()\n\t\t{");
        }
        public static void WriteFoot()
        {
            Writer.WriteLine("\t\t}\n\t}\n}");
        }


        public static void WriteRegistration(AssetsGroupConfig config, string assetName, string assetFullPath)
        {
            assetFullPath = GenManager.IsDebug ? Path.GetFullPath(assetFullPath).Replace("\\", "\\\\") : string.Empty;
            Writer.WriteLine($"\t\t\tAssetsManager.Register<{config.LoadAs}>(\"{config.Method}\", \"{assetName}\", \"{assetFullPath}\");");
        }
    }
}
