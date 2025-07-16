using Fenzwork.GenLib;
using Fenzwork.GenLib.Models;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;
using MonoGame.Framework.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Fenzwork.Systems.Assets;
using System.Diagnostics;
using System.Linq;
using Fenzwork.Ase2Png;
using System.Threading.Tasks;

namespace Fenzwork.Desktop
{
    public static class AssetsDebugger
    {
        internal static Dictionary<FileSystemWatcher, AssetsAssembly> _WatchersAssemblies = [];

        internal static Dictionary<string, MainConfig> _ConfigCache = [];

        internal static bool _IsAse2PngOn = false;

        public static void Init()
        {
            AssetsManager.DebuggerInitMethod = SetupDebugAssetsAsm;
            AssetsManager.DebuggerTickMethod = Tick;

            VerifyAse2Png();
        }

        private static void VerifyAse2Png()
        {
            try
            {
                AseProcessor.Dummy();

                // if no error happened then that means Ase2Png does exist
                _IsAse2PngOn = true;
            }
            catch { }
        }


        internal static void SetupDebugAssetsAsm(AssetsAssembly asm)
        {
            if (!_ConfigCache.ContainsKey(asm.AssetsConfigFile))
            {
                var configFile = File.OpenRead(asm.AssetsConfigFile);
                var configObj = JsonSerializer.Deserialize<MainConfig>(configFile);
                configObj.AssetsDirectoryName = configObj.AssetsDirectoryName.TrimEnd('/','\\');
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
        internal static ConcurrentDictionary<string, ConcurrentBag<AssetsHotreloadRequest>> PendingRegisteringAfter = [];

        internal static void Tick()
        {
            while (PendingRequest.TryDequeue(out var request))
            {
                if (request.IsRegisterNotUnregister)
                {
                    AssetsManager.Register(request.Asm, request.LoadType, request.GroupConfig.Method, request.FileName, request.FilePath, request.AdditionalParam);

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
                if (!IsNotAsset(e.OldFullPath, e.OldName))
                    ValidatedRequestForUnregister(e.OldName, e.OldFullPath, asm);
                if (!IsNotAsset(e.FullPath, e.Name))
                    ValidatedRequestForRegister(e.Name, e.FullPath, asm);
            }
            
        }

        private static void File_Changed(object sender, FileSystemEventArgs e)
        {
            if (_WatchersAssemblies.TryGetValue((FileSystemWatcher)sender, out var asm) && !IsNotAsset(e.FullPath, e.Name))
            {
                ValidatedRequestForUnregister(e.Name, e.FullPath, asm);
                ValidatedRequestForRegister(e.Name, e.FullPath, asm);
            }
        }

        private static void File_Created(object sender, FileSystemEventArgs e)
        {
            //if (_WatchersAssemblies.TryGetValue((FileSystemWatcher)sender, out var asm))
            //{
            //    ValidatedRequestForRegister(e.Name, e.FullPath, asm);
            //}
        }

        private static bool IsNotAsset(string fullPath, string relativePath)
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

        private static void Ase2Png(string assetPath)
        {
            try
            {
                AseProcessor.Process(assetPath, $"{Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath))}.png");
            } catch { }
        }

        private static void ValidatedRequestForRegister(string assetName, string assetPath, AssetsAssembly asm, ConfigMatchResult? configMatchResult = null)
        {
            if (_IsAse2PngOn && assetName.EndsWith(".ase"))
            {
                Ase2Png(assetPath);
                return;
            }

            var config = _ConfigCache[asm.AssetsConfigFile];

            configMatchResult ??= Utilities.FileMatchesConfig(asm.WorkingDirectory, assetPath, config);

            // It matches one of the groups
            if (configMatchResult.HasValue)
            {
                var result = configMatchResult.Value;

                var type = Type.GetType(result.GroupConfig.LoadAs);

                Directory.CreateDirectory(Path.Combine(asm.WorkingDirectory, asm.BuildOutputDirectory));

                var isPack = result.GroupConfig.Method == "pack";
                var packMetadataPath = isPack ? Path.Combine(
                            result.LocalDir[..^(result.GroupConfig.From.Length + 1)], // This will try to get the Assets path with current domain enabled
                            result.GroupConfig.PackConfig.PackInto.TrimEnd('/', '\\'),
                            result.GroupConfig.PackConfig.MetadataName
                        ) : null;

                var aditionalParams = isPack ? 
                        Path.GetRelativePath(asm.WorkingDirectory, packMetadataPath).Replace('\\', '/') : null;

                var request = new AssetsHotreloadRequest(true, assetName.Replace('\\', '/'), assetPath, asm, type, result.GroupConfig, aditionalParams);

                if (result.GroupConfig.Method == "build")
                    BuildAsset(asm.WorkingDirectory, asm.BuildIntermediateDirectory, assetName, assetPath, config, result.GroupConfig, asm.BuildOutputDirectory);
                else if (isPack)
                {
                    var updateType = RegenerateSpritesAtlases(asm.WorkingDirectory, config, result.GroupConfig, result.GroupFiles);
                    if (updateType == AtlasUpdateType.Hard)
                        PendingRegisteringAfter.AddOrUpdate(packMetadataPath, [request], (_, list) => { list.Add(request); return list; });
                    else if (updateType == AtlasUpdateType.Soft)
                        PendingRequest.Enqueue(request);

                    return;
                }

                PendingRequest.Enqueue(request);

                if (PendingRegisteringAfter.Remove(request.FilePath, out var spriteRegRequests))
                {
                    while (spriteRegRequests.TryTake(out var spriteRequest))
                    {
                        PendingRequest.Enqueue(spriteRequest);
                    }

                }
            }

        }

        private static void ValidatedRequestForUnregister(string assetName, string assetPath, AssetsAssembly asm)
        {
            if (AssetsManager.DebugPaths.ContainsKey(assetPath))
                PendingRequest.Enqueue(new AssetsHotreloadRequest(false, assetName.Replace('\\', '/'), assetPath, asm, null, null));

        }

        private static AtlasUpdateType RegenerateSpritesAtlases(string workingDir, MainConfig mainConfig, AssetsGroupConfig config, IEnumerable<string> groupFiles)
        {
            var atlasPacker = new AtlasPacker
            {
                WorkingDir = workingDir,
                Config = config.PackConfig,
                SpritesCacheFilePath = Path.Combine(workingDir, "obj", ".AtlasPacker", $"{mainConfig.AssetsDirectoryName}.{config.PackConfig.PackInto.Replace('/', '.').TrimEnd('.')}.cache")
            };
            atlasPacker.Begin();
            foreach (var sprite in groupFiles)
                atlasPacker.AddSprite(Path.GetRelativePath(workingDir, sprite).Replace('\\', '/'), sprite);
            atlasPacker.Generate();
            atlasPacker.End();
            return atlasPacker.UpdateType;
        }

        private static bool IsProbablyTemp(string path) =>  path.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)
                                                            || path.EndsWith('~');
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

    public record struct AssetsHotreloadRequest(bool IsRegisterNotUnregister, string FileName, string FilePath, AssetsAssembly Asm, Type LoadType, AssetsGroupConfig GroupConfig, object AdditionalParam = null);
    
}
