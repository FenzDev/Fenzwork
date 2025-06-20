using Fenzwork.GenLib.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Fenzwork.GenLib;

public static class GenManager
{
    public static string AssetsConfigFile = "";
    public static string AssetsBaseDir = "";
    public static string IntermidateDir = "";
    public static string MGCBFileName = "";
    public static string Namespace = "";
    public static bool IsDebug;

    const string AssetsClassFileName = "Assets.g.cs";

    internal static string AssetsDirectory;

    /// <summary>
    /// Start full generation of MGCB, Assets class and more.
    /// </summary>
    public static void Start()
    {
        // 1) We read the mainConfig and deserialize it
        var configFile = File.OpenRead(AssetsConfigFile);
        if (configFile == null)
            throw new Exception($"Couldn't open the config file ({AssetsConfigFile})");

        var mainConfig = JsonSerializer.Deserialize<MainConfig>(configFile);
        if (mainConfig == null)
            throw new Exception($"Couldn't desirealize the config file ({AssetsConfigFile})");

        configFile.Close();

        // 2) We open the generated files 
        AssetsDirectory = Path.Combine( AssetsBaseDir, mainConfig.AssetsDirectoryName);
        Directory.CreateDirectory(AssetsDirectory);

        var mgcbFile = File.OpenWrite(Path.Combine(AssetsDirectory, MGCBFileName));
        var mgcbWriter = new StreamWriter(mgcbFile);
        var assetsClassFile = File.OpenWrite(Path.Combine(IntermidateDir, AssetsClassFileName));
        var assetsClassWriter = new StreamWriter(assetsClassFile);
        
        MGCBGenerator.Writer = mgcbWriter;
        MGCBGenerator.Main = mainConfig;
        AssetsClassGenerator.Writer = assetsClassWriter;
        AssetsRegistryClassGenerator.Writer = assetsClassWriter;

        // 3) We write the header of MGCB
        MGCBGenerator.WriteHeader();
        AssetsRegistryClassGenerator.WriteHead(mainConfig);

        // 4 ) We loop through files
        // We search for the assets either from the Assets folder or from inside of its top directories
        // Those directories are called Domain Directories.
        if (mainConfig.EnableDomainFolders)
            foreach (var assetGroupConfig in mainConfig.Assets)
            {
                foreach (var dir in Directory.EnumerateDirectories(AssetsDirectory))
                {
                    if (assetGroupConfig.Method.Equals("ignore"))
                        continue;

                    GenerateFromThisDirectory(Path.Combine(dir, assetGroupConfig.From), mainConfig, assetGroupConfig);
                }
            }
        else
            foreach (var assetGroupConfig in mainConfig.Assets)
            {
                if (assetGroupConfig.Method.Equals("ignore"))
                    continue;

                GenerateFromThisDirectory(Path.Combine(AssetsDirectory, assetGroupConfig.From), mainConfig, assetGroupConfig);
            }


        // 5) We write the foot of Assets Registry then below we write the Assets Class
        AssetsRegistryClassGenerator.WriteFoot();
        // TODO : Make it optional to generate the Assets class
        AssetsClassGenerator.WriteClass();

        // N-1) We close the generated files flushing the buffer !
        mgcbFile.SetLength(mgcbFile.Position);
        mgcbWriter.Close();
        assetsClassFile.SetLength(assetsClassFile.Position);
        assetsClassWriter.Close();

    }
    
    static void GenerateFromThisDirectory(string thisDir, MainConfig mainConfig, AssetsGroupConfig thisGroupConfig)
    {
        if (!Directory.Exists(thisDir))
            return;
        // foreach of the files matching Include patterns
        foreach (var file in thisGroupConfig.Include.SelectMany(pattern => Directory.EnumerateFiles(thisDir, pattern)))
        {
            var assetName = Path.GetRelativePath(AssetsDirectory, file).Replace('\\','/');
            // Append to the mgcb file
            MGCBGenerator.WriteAsset(thisGroupConfig, assetName);
            // Append to the registry
            AssetsRegistryClassGenerator.WriteRegistration(thisGroupConfig, assetName);
            // Include this to the Assets class node tree
            AssetsClassGenerator.Include(thisGroupConfig, assetName);
        }    
    }


}
