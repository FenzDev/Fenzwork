using Fenzwork.GenLib;
using Fenzwork.GenLib.Models;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;
using MonoGame.Framework.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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
                if (IsProbablyTemp(request.FilePath))
                    continue;

                if (!File.Exists(request.FilePath))
                    continue;

                if (request.FileName.StartsWith($"bin{Path.DirectorySeparatorChar}") ||
                    request.FileName.StartsWith($"obj{Path.DirectorySeparatorChar}"))
                    continue;

                if (request.IsRegisterNotUnregister)
                {
                    var posibleResult = Utilities.FileMatchesConfig(request.Asm.WorkingDirectory, request.FilePath, request.AssetsConfig);
                    if (posibleResult.HasValue)
                    {
                        var result = posibleResult.Value;
                        var type = Type.GetType(result.GroupConfig.LoadAs);
                        
                        var objDir = Path.Combine("obj", "Debug");
                        var binDir = Path.Combine("bin", PlatformInfo.MonoGamePlatform.ToString(), ".mgcref");
                        Directory.CreateDirectory(Path.Combine(request.Asm.WorkingDirectory, objDir));
                        Directory.CreateDirectory(Path.Combine(request.Asm.WorkingDirectory, binDir));

                        if (result.GroupConfig.Method == "build")
                            ExecuteMGCB(
                                workingDir: request.Asm.WorkingDirectory,
                                intermidateDir: objDir,
                                assetName: result.AssetName,
                                mainConfig: request.AssetsConfig,
                                groupConfig: result.GroupConfig,
                                outputDir: binDir);

                        AssetsManager.Register(request.Asm, type, result.GroupConfig.Method, result.AssetName, request.FilePath);
                    }
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
                PendingRequest.Enqueue(new(false, e.Name, e.FullPath, asm, _ConfigCache[asm.AssetsConfigFile]));
            }
        }

        private static void File_Renamed(object sender, RenamedEventArgs e)
        {
            if (_WatchersAssemblies.TryGetValue((FileSystemWatcher)sender, out var asm))
            {
                PendingRequest.Enqueue(new(false, e.Name, e.OldFullPath, asm, _ConfigCache[asm.AssetsConfigFile]));
                PendingRequest.Enqueue(new(true, e.Name, e.FullPath, asm, _ConfigCache[asm.AssetsConfigFile]));
            }
            
        }

        private static void File_Changed(object sender, FileSystemEventArgs e)
        {
            if (_WatchersAssemblies.TryGetValue((FileSystemWatcher)sender, out var asm))
            {
                PendingRequest.Enqueue(new(false, e.Name, e.FullPath, asm, _ConfigCache[asm.AssetsConfigFile]));
                PendingRequest.Enqueue(new(true, e.Name, e.FullPath, asm, _ConfigCache[asm.AssetsConfigFile]));
            }
        }

        private static void File_Created(object sender, FileSystemEventArgs e)
        {
            if (_WatchersAssemblies.TryGetValue((FileSystemWatcher)sender, out var asm))
            {
                PendingRequest.Enqueue(new(true, e.Name, e.FullPath, asm, _ConfigCache[asm.AssetsConfigFile]));
            }
        }

        private static bool IsProbablyTemp(string path) =>  path.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)
                                                            || path.EndsWith("~");
        private static readonly TimeSpan MGCBTimeout = TimeSpan.FromSeconds(10);
        
        private static void ExecuteMGCB(string workingDir, string intermidateDir, string assetName, MainConfig mainConfig, AssetsGroupConfig groupConfig, string outputDir )
        {
            var refs = string.Join("", mainConfig.BuildReferences.Select(reference => $"/reference:{reference} "));
            var buildFullParams = $"/importer:{groupConfig.BuildImporter} /processor:{groupConfig.BuildProcessor} {string.Join("", mainConfig.BuildReferences.Select(param => $"/processorParam:{param} "))}";

            var builder = new PipelineManager(workingDir, Path.Combine(workingDir, outputDir), Path.Combine(workingDir, intermidateDir)) { Platform = Enum.Parse<TargetPlatform>(mainConfig.BuildPlatform) };
            
            var dict = new OpaqueDataDictionary();
            // TODO: TEMP
            foreach(var processorParam in groupConfig.BuildProcessorParams)
            {
                var paramPair = processorParam.Split("=");
                var paramKey = paramPair[0];
                var paramValue = paramPair[1];

                if (int.TryParse(paramValue, out var intParam))
                    dict.Add(paramKey, intParam);
                else if (float.TryParse(paramValue, out var floatParam))
                    dict.Add(paramKey, floatParam);
                else if (bool.TryParse(paramKey, out var boolParam))
                    dict.Add(paramKey, boolParam);
                else
                    dict.Add(paramKey, paramValue);
            }

            try
            {
                var something = builder.BuildContent(Path.Combine(workingDir,assetName),
                    importerName: groupConfig.BuildImporter,
                    processorName: groupConfig.BuildProcessor,
                    processorParameters: dict
                    );

            }
            catch
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "dotnet",
                    Arguments = $"mgfxc.dll \"{Path.Combine(workingDir, assetName)}\" \"{Path.Combine(workingDir, outputDir, assetName)}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }).WaitForExit();
            }
            
        }
    }

    public record struct AssetsHotreloadRequest(bool IsRegisterNotUnregister, string FileName, string FilePath, AssetsAssembly Asm, MainConfig AssetsConfig);
}
