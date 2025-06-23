using Fenzwork.GenLib;
using Fenzwork.GenLib.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                EnableRaisingEvents = true
            };
            watcher.Created += File_Created;
            watcher.Changed += File_Changed;
            watcher.Renamed += File_Renamed;
            watcher.Deleted += File_Deleted;

            _WatchersAssemblies.Add(watcher, asm);
        }

        internal static ConcurrentQueue<AssetsDebugRequest> PendingRequest = [];
        internal static void Tick()
        {
            while (PendingRequest.TryDequeue(out var request))
            {
                if (request.IsRegisterNotUnregister)
                {
                    var result = Utilities.FileMatchesConfig(request.Asm.WorkingDirectory, request.FilePath, request.AssetsConfig);
                    if (result.HasValue)
                    {
                        var type = Type.GetType(result.Value.LoadAs);

                        AssetsManager.Register(request.Asm, type, result.Value.Method, result.Value.AssetName, request.FilePath);
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
            
        }

        private static void File_Renamed(object sender, RenamedEventArgs e)
        {
            if (_WatchersAssemblies.TryGetValue((FileSystemWatcher)sender, out var asm))
            {
                PendingRequest.Enqueue(new(false, e.OldFullPath, asm, _ConfigCache[asm.AssetsConfigFile]));
                PendingRequest.Enqueue(new(true, e.FullPath, asm, _ConfigCache[asm.AssetsConfigFile]));
            }
            
        }

        private static void File_Changed(object sender, FileSystemEventArgs e)
        {
            if (AssetsManager.DebugPaths.TryGetValue(e.FullPath, out var assetRoot))
            {
                var source = _WatchersAssemblies[(FileSystemWatcher)sender];
                if (assetRoot.IsLoaded && assetRoot.Source == source)
                {
                    // hot reload
                    AssetsManager.UnloadAsset(assetRoot);
                    AssetsManager.LoadAsset(assetRoot);
                }
            }
        }

        private static void File_Created(object sender, FileSystemEventArgs e)
        {
        }

    }

    public record struct AssetsDebugRequest(bool IsRegisterNotUnregister, string FilePath, AssetsAssembly Asm, MainConfig AssetsConfig);
}
