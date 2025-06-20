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
            var workingDirectory = GenManager.IsDebug? $"{GenManager.AssetsDirectory.Replace("\\","\\\\")}": "";
            Writer.WriteLine($"\t\tprivate const string DebugWorkingDirectory = \"{workingDirectory}\";");
            Writer.WriteLine($"\t\tprivate const string AssetsDirectoryName = \"{mainConfig.AssetsDirectoryName}\";");
            Writer.WriteLine("\t\tprivate static void Register()\n\t\t{");
        }
        public static void WriteFoot()
        {
            Writer.WriteLine("\t\t}\n\t}\n}");
        }


        public static void WriteRegistration(AssetsGroupConfig config, string assetName)
        {
            Writer.WriteLine($"\t\t\tAssetsManager.Register<{config.LoadAs}>(\"{config.Method}\", \"{assetName}\");");
        }
    }
}
