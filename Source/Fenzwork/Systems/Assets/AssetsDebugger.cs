using Fenzwork.AssetsLibrary;
using Fenzwork.AssetsLibrary.Models;
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
        internal static void Tick()
        {
            while (PendingAssetUpdate.TryDequeue(out var update))
            {
                var assetSource = new AssetSourceInfo(update.AsmDirectory, update.Info);
                switch (update.Type)
                {
                    case AssetUpdateType.Changed:
                        var assetRoot = AssetsManager.GetRoot(new AssetID(update.Info.AssetName, update.Info.Domain, AssetsManager.GetType(update.Info.Type)));
                        AssetsManager.ReloadAsset(assetRoot);
                        break;
                    case AssetUpdateType.Created:
                        AssetsManager.RegisterAsset(new AssetSourceInfo(update.AsmDirectory, update.Info));
                        
                        break;
                    case AssetUpdateType.Deleted:
                        AssetsManager.UnregisterAsset(new AssetSourceInfo(update.AsmDirectory, update.Info));
                        break;
                }
            }
        }

        internal static ConcurrentDictionary<FileSystemWatcher, DebugAssetsInfo> DebugSources = new();

        internal static ConcurrentQueue<AssetUpdateInfo> PendingAssetUpdate = new();

        public static void AddDebugAssetsInfo(string configFile, string assetsDirectory)
        {
            using var file = File.OpenRead(configFile);

            var config = JsonSerializer.Deserialize<MainConfig>(file, new JsonSerializerOptions() { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip });

            var watcher = new FileSystemWatcher(assetsDirectory)
            {
                NotifyFilter = NotifyFilters.LastWrite|NotifyFilters.CreationTime,
                EnableRaisingEvents = true,
            };

            watcher.Renamed += AssetWasRenamed;
            watcher.Created += AssetWasCreated;
            watcher.Changed += AssetWasChanged;
            watcher.Deleted += AssetWasDeleted;
            
            if (!DebugSources.TryAdd(watcher, new (config, AssetsManager.SelectedAssetsAssemblyDirectory, assetsDirectory, Path.GetRelativePath(Path.GetDirectoryName(configFile), assetsDirectory), watcher)))
                watcher.Dispose();
        }

        static AssetInfo? GetAssetInfo(string assetFullPath, FileSystemWatcher watcher)
        {
            return GetAssetInfo(assetFullPath, DebugSources[watcher]);
        }
        static AssetInfo? GetAssetInfo(string assetFullPath, DebugAssetsInfo info)
        {
            return Utilities.GetAssetInfo(info.Config, assetFullPath, info.AssetsFullPath);
        }
        static void Enqueue(AssetUpdateType type, string AsmDirectory, object watcher, string assetFullPath)
        {
            var assetInfo = GetAssetInfo(assetFullPath, (FileSystemWatcher)watcher);
            if (assetInfo.HasValue)
                PendingAssetUpdate.Enqueue(new(type, assetFullPath, , assetInfo.Value));
        }
        private static void AssetWasDeleted(object sender, FileSystemEventArgs e)
        {
            Enqueue(AssetUpdateType.Deleted,  sender, e.FullPath);
        }

        private static void AssetWasChanged(object sender, FileSystemEventArgs e)
        {
            Enqueue(AssetUpdateType.Changed, sender, e.FullPath);
        }

        private static void AssetWasCreated(object sender, FileSystemEventArgs e)
        {
            Enqueue(AssetUpdateType.Created, sender, e.FullPath);
        }

        private static void AssetWasRenamed(object sender, RenamedEventArgs e)
        {
            Enqueue(AssetUpdateType.Deleted, sender, e.OldFullPath);
            Enqueue(AssetUpdateType.Created, sender, e.FullPath);
        }
    }
    public enum AssetUpdateType
    {
        Changed,
        Created,
        Deleted
    }
    public record struct AssetUpdateInfo(AssetUpdateType Type, string AssetFullPath, string AsmDirectory, AssetInfo Info);
    public record struct DebugAssetsInfo(MainConfig Config, string AsmDirectory, string AssetsFullPath, string AssetsPath, FileSystemWatcher Watcher);
}
