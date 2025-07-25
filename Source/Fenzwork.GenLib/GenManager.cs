﻿using Fenzwork.GenLib.Models;
using System.Diagnostics;
using System.Linq.Expressions;
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
    internal static AtlasPacker AtlasPacker = new();

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
        Utilities.GoThroughConfig(mainConfig, AssetsDirectory, (localDir, groupConfig, files) =>
        {
            groupConfig.LoadAs = groupConfig.LoadAs.Split(", ")[0];
            GenerateFromThisGroup(localDir, mainConfig, groupConfig, files);
            return true;   
        });


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
    
    static void GenerateFromThisGroup(string thisDir, MainConfig mainConfig, AssetsGroupConfig thisGroupConfig, IEnumerable<string> files)
    {
        var isPack = thisGroupConfig.Method == "pack";

        if (isPack)
        {
            if (thisGroupConfig.PackConfig == null)
            {
                Console.WriteLine("Pack config was not specified.");
                return;
            }

            thisGroupConfig.PackConfig._MetadataFullName = Path.Combine(thisGroupConfig.PackConfig.PackInto, thisGroupConfig.PackConfig.MetadataName).Replace('\\','/');

            // We Begin AtlasPacker
            thisGroupConfig.PackConfig.PackInto = thisGroupConfig.PackConfig.PackInto.Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\','/');
            AtlasPacker.WorkingDir = AssetsDirectory;
            AtlasPacker.Config = thisGroupConfig.PackConfig;
            AtlasPacker.SpritesCacheFilePath = Path.Combine(AssetsDirectory, "obj", ".AtlasPacker", $"{mainConfig.AssetsDirectoryName}.{thisGroupConfig.PackConfig.PackInto.Replace('/','.')}.cache");
            AtlasPacker.Begin();
        }

        // foreach of the files matching Include patterns
        foreach (var file in files)
        {
            var assetName = Path.GetRelativePath(AssetsDirectory, file).Replace('\\','/');
            // Append to the mgcb file
            MGCBGenerator.WriteAsset(thisGroupConfig, assetName);
            // Append to the registry
            AssetsRegistryClassGenerator.WriteRegistration(thisGroupConfig, assetName, file);
            // Include this to the Assets class node tree
            AssetsClassGenerator.Include(thisGroupConfig, assetName);
            // Add Sprite to pack if method was pack
            if (isPack)
                AtlasPacker.AddSprite(assetName, file);
        }
        if (isPack)
        {
            AtlasPacker.Generate();
            AtlasPacker.End();
        }
    }


}
