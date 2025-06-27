using Fenzwork.GenLib;
using Fenzwork.GenLib.Models;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Content.Pipeline.Builder;
using MonoGame.Framework.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Fenzwork.Systems.Assets
{
    public static class AssetsDebugger
    {
        internal static Dictionary<FileSystemWatcher, AssetsAssembly> _WatchersAssemblies = [];

        internal static Dictionary<string, MainConfig> _ConfigCache = [];

        internal static void SetupDebugAssetsAsm(AssetsAssembly asm)
        {
            if (!_ConfigCache.ContainsKey(asm.AssetsConfigFile))
            {
                var configFile = File.OpenRead(asm.AssetsConfigFile);
                var configObj = JsonSerializer.Deserialize<MainConfig>(configFile);
                configFile.Close();
                _ConfigCache.Add(asm.AssetsConfigFile, configObj);
            }

            var watcher = new FileSystemWatcher(asm.WorkingDirectory)
            {
                NotifyFilter = NotifyFilters.FileName|NotifyFilters.LastAccess,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            watcher.Created += File_Created;
            watcher.Changed += File_Changed;
            watcher.Renamed += File_Renamed;
            watcher.Deleted += File_Deleted;

            _WatchersAssemblies.Add(watcher, asm);
        }

        internal static ConcurrentQueue<AssetsHotreloadRequest> PendingRequest = [];
        internal static void Tick()
        {
            while (PendingRequest.TryDequeue(out var request))
            {
                if (request.IsRegisterNotUnregister)
                {
                    AssetsManager.Register(request.Asm, request.LoadType, request.GroupConfig.Method, request.FileName, request.FilePath);
                }
                else
                {
                    if (AssetsManager.DebugPaths.Remove(request.FilePath, out var assetRoot))
                    {
                        AssetsManager.Unregister(assetRoot, request.Asm);

                    }
                }
                   
                
            }
        }

        

        private static void File_Deleted(object sender, FileSystemEventArgs e)
        {
            if (_WatchersAssemblies.TryGetValue((FileSystemWatcher)sender, out var asm))
            {
                ValidatedRequestForUnregister(e.Name, e.FullPath, asm);
            }
        }

        private static void File_Renamed(object sender, RenamedEventArgs e)
        {
            if (_WatchersAssemblies.TryGetValue((FileSystemWatcher)sender, out var asm))
            {
                ValidatedRequestForUnregister(e.OldName, e.OldFullPath, asm);
                ValidatedRequestForRegister(e.Name, e.FullPath, asm);
            }
            
        }

        private static void File_Changed(object sender, FileSystemEventArgs e)
        {
            if (_WatchersAssemblies.TryGetValue((FileSystemWatcher)sender, out var asm))
            {
                ValidatedRequestForUnregister(e.Name, e.FullPath, asm);
                ValidatedRequestForRegister(e.Name, e.FullPath, asm);
            }
        }

        private static void File_Created(object sender, FileSystemEventArgs e)
        {
            if (_WatchersAssemblies.TryGetValue((FileSystemWatcher)sender, out var asm))
            {
                ValidatedRequestForRegister(e.Name, e.FullPath, asm);
            }
        }

        private static bool IsProbablyNotAsset(string fullPath, string relativePath)
        {
            if (relativePath.StartsWith($"bin{Path.DirectorySeparatorChar}") ||
                relativePath.StartsWith($"obj{Path.DirectorySeparatorChar}"))
                return true;

            if (!File.Exists(fullPath)) 
                return true;

            if (IsProbablyTemp(relativePath))
                return true;

            const FileAttributes nonWantedAttributes = FileAttributes.Hidden | FileAttributes.Temporary;

            if ((File.GetAttributes(fullPath) & nonWantedAttributes) == nonWantedAttributes)
                return true;

            return false;
        }
        
        private static void ValidatedRequestForRegister(string assetName, string assetPath, AssetsAssembly asm)
        {
            if (IsProbablyNotAsset(assetPath, assetName))
                return;

            var config = _ConfigCache[asm.AssetsConfigFile];
            var posibleResult = Utilities.FileMatchesConfig(asm.WorkingDirectory, assetPath, config);
            // It matches on of the groups
            if (posibleResult.HasValue)
            {
                var result = posibleResult.Value;

                var type = Type.GetType(result.GroupConfig.LoadAs);

                var objDir = Path.Combine(asm.WorkingDirectory, "obj", PlatformInfo.MonoGamePlatform.ToString(), "net8.0", ".mgcref");
                var binDir = Path.Combine(asm.WorkingDirectory, "bin", PlatformInfo.MonoGamePlatform.ToString(), ".mgcref");
                Directory.CreateDirectory(Path.Combine(asm.WorkingDirectory, binDir));

                if (result.GroupConfig.Method == "build")
                    BuildAsset(asm.WorkingDirectory, objDir, assetName, assetPath, config, result.GroupConfig, binDir);

                PendingRequest.Enqueue(new (true, assetName.Replace('\\','/'), assetPath, asm, type, result.GroupConfig));
            }

        }
        private static void ValidatedRequestForUnregister(string assetName, string assetPath, AssetsAssembly asm)
        {
            if (IsProbablyNotAsset(assetPath, assetName))
                return;

            if (AssetsManager.DebugPaths.ContainsKey(assetPath))
                PendingRequest.Enqueue(new AssetsHotreloadRequest(false, assetName.Replace('\\', '/'), assetPath, asm, null, null));

        }

        private static bool IsProbablyTemp(string path) =>  path.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)
                                                            || path.EndsWith("~");
        private static void BuildAsset(string workingDir, string intermidateDir, string assetName, string assetFullPath, MainConfig mainConfig, AssetsGroupConfig groupConfig, string outputDir )
        {
            var builder = new PipelineManager(workingDir, outputDir, intermidateDir) { Platform = Enum.Parse<TargetPlatform>(mainConfig.BuildPlatform) };
            
            var dict = new OpaqueDataDictionary();

            foreach(var processorParam in groupConfig.BuildProcessorParams)
            {
                var paramPair = processorParam.Split("=");
                var paramKey = paramPair[0].TrimEnd();
                var paramValue = paramPair[1].TrimStart();

                if (int.TryParse(paramValue, out var intParam))
                    dict.Add(paramKey, intParam);
                else if (float.TryParse(paramValue, out var floatParam))
                    dict.Add(paramKey, floatParam);
                else if (bool.TryParse(paramValue, out var boolParam))
                    dict.Add(paramKey, boolParam);
                else
                    dict.Add(paramKey, paramValue);
            }

            var pipelineEvent = builder.BuildContent(assetFullPath,
                importerName: groupConfig.BuildImporter,
                processorName: groupConfig.BuildProcessor,
                processorParameters: dict
                );
            

        }
    }

    public record struct AssetsHotreloadRequest(bool IsRegisterNotUnregister, string FileName, string FilePath, AssetsAssembly Asm, Type LoadType, AssetsGroupConfig GroupConfig);
    
}
