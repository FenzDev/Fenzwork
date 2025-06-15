using Fenzwork.AssetsLibrary.Models;
using Fenzwork.Services;
using Fenzwork.Systems.Assets;
using Fenzwork.Systems.Assets.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork
{
    public static class AssetsManager
    {
        internal static void Init()
        {
            CustomLoaders.Add(typeof(object), new JsonLoader());
            CustomLoaders.Add(typeof(string), new TextLoader());
            
            foreach (var registrations in RegestrationsQueue)
            {
                foreach (var registration in registrations)
                {
                    RegisterAsset(new ("", new(registration)));
                }
            }
        }

        public static string SelectedAssetsAssemblyDirectory;
        public static List<string> DifferentAssetsAssembeliesOrders = [""];

        public static AssetLoadingTime LoadingTime { get; internal set; } = AssetLoadingTime.Lazy;

        internal static Dictionary<AssetID, AssetRoot> AssetsBank { get; } = [];
        public static Dictionary<Type, AssetCustomLoader> CustomLoaders { get; } = [];


        public static AssetHandle<T> Get<T>(string assetname, string domain = "") => Get<T>(new (assetname, domain, typeof(T)));
        public static AssetHandle<T> Get<T>(AssetID id)
        {
            var assetRoot = GetRoot(id);

            if (!assetRoot.IsLoaded && !assetRoot.IsRegistered && LoadingTime == AssetLoadingTime.Lazy)
                LoadAsset(assetRoot);

            return assetRoot.OpenHandle<T>();
        }
        internal static AssetRoot GetRoot(AssetID id)
        {
            if (AssetsBank.TryGetValue(id, out var root))
            {
                return root;
            }
            else
            {
                var newRoot = new AssetRoot(id);
                
                AssetsBank.Add(id, newRoot);

                return newRoot;
            }
        }

        internal static Queue<string[]> RegestrationsQueue = new();
        internal static void RegisterAssetsEnqueue(string[] info)
        {
            RegestrationsQueue.Enqueue(info);
        }
        
        internal static Dictionary<string, Type> TypesCache = [ ];
        internal static Type GetType(string typeName)
        {
            if (TypesCache.TryGetValue(typeName, out var typeCache))
                return typeCache;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(typeName);

                if (type == null)
                    continue;

                TypesCache.Add(typeName, type);
                return type;
            }

            throw new Exception($"Couldn't find type with name {typeName}.");
        }
        
        internal static AssetRoot FindAssetRootByInfo(AssetSourceInfo info)
        {
            var assetID = new AssetID(info.Info.AssetName, info.Info.Domain, GetType(info.Info.Type));
            return GetRoot(assetID);

        }

        internal static void RegisterAsset(AssetSourceInfo info)
        {
            var assetRoot = FindAssetRootByInfo(info);

            if (assetRoot.Sources.Contains(info))
                return;

            assetRoot.Sources.Add(info);
        }

        internal static void UnregisterAsset(AssetSourceInfo info)
        {
            var assetRoot = FindAssetRootByInfo(info);
            
            var idx = assetRoot.Sources.IndexOf(info);

            if (idx < 0)
                return;

            var mustUnload = assetRoot.IsLoaded && idx == assetRoot.Sources.Count - 1;

            if (mustUnload)
                UnloadAsset(assetRoot);

            assetRoot.Sources.RemoveAt(idx);

            if (assetRoot.IsRegistered && mustUnload)
                LoadAsset(assetRoot);
        }

        internal static void UnloadAsset(AssetRoot assetRoot)
        {
            if (assetRoot.Loader != null)
            {
                assetRoot.Loader.DoUnLoad(assetRoot.ID, assetRoot.CurrentSource.Info.Parameter, assetRoot.Content);
                return;
            }

            if (assetRoot.CurrentSource.Info.Method == "build")
                UnloadBuildAsset(assetRoot);

            assetRoot.Content = null;
        }

        internal static void LoadAsset(AssetRoot assetRoot)
        {
            if (assetRoot.IsLoaded && !assetRoot.IsRegistered) return;

            switch (assetRoot.CurrentSource.Info.Method)
            {
                case "build":
                    LoadBuildAsset(assetRoot);
                    break;
                case "copy":
                    LoadCopyAsset(assetRoot);
                    break;
            }
        }

        internal static void ReloadAsset(AssetRoot assetRoot)
        {
            UnloadAsset(assetRoot);
            LoadAsset(assetRoot);
        }

        internal static void LoadCopyAsset(AssetRoot assetRoot)
        {
            var source = assetRoot.CurrentSource;

            var assetPath = source.AssemblyRelativeLocation == null ?
                Path.Combine(source.Info.AssetPath, source.Info.AssetPath) 
                : Path.Combine(source.AssemblyRelativeLocation, source.Info.AssetsPath, source.Info.AssetPath);
            using var stream = TitleContainer.OpenStream(assetPath);
            try
            {
                if (CustomLoaders.TryGetValue(assetRoot.ID.AssetType, out var loader))
                {
                    loader.DoLoad(stream, assetRoot.ID, source.Info.Parameter, out var assetContent);

                    assetRoot.Content = assetContent;
                    assetRoot.IsLoaded = true;
                }
            }
            catch { }

        }

        internal static void UnloadBuildAsset(AssetRoot assetRoot)
        {
            var source = assetRoot.CurrentSource;
            var dir = source.AssemblyRelativeLocation == "" ? source.Info.AssetsPath : Path.Combine(source.AssemblyRelativeLocation, source.Info.AssetsPath);

            var oldDir = MGGame.Instance.Content.RootDirectory;
            MGGame.Instance.Content.RootDirectory = dir;

            var contentName = Path.Combine(Path.GetDirectoryName(source.Info.AssetPath), Path.GetFileNameWithoutExtension(source.Info.AssetPath));

            MGGame.Instance.Content.UnloadAsset(contentName);

            assetRoot.IsLoaded = true;

            MGGame.Instance.Content.RootDirectory = oldDir;
        }

        internal static void LoadBuildAsset(AssetRoot assetRoot)
        {
            var source = assetRoot.CurrentSource;
            var dir = source.AssemblyRelativeLocation == ""? source.Info.AssetsPath: Path.Combine(source.AssemblyRelativeLocation, source.Info.AssetsPath);

            var oldDir = MGGame.Instance.Content.RootDirectory;
            MGGame.Instance.Content.RootDirectory = dir;

            var contentName = Path.Combine(Path.GetDirectoryName(source.Info.AssetPath), Path.GetFileNameWithoutExtension(source.Info.AssetPath));
            assetRoot.Content = GetGenericMethod(typeof(ContentManager), "Load", assetRoot.ID.AssetType).Invoke(MGGame.Instance.Content, [contentName]);


            assetRoot.IsLoaded = true;

            MGGame.Instance.Content.RootDirectory = oldDir;
        }

        static MethodInfo GetGenericMethod(Type mainType, string methodName, Type insideType)
        {
            // Get the method info for your generic method
            MethodInfo method = mainType.GetMethod(methodName);

            // Make it generic with your runtime type
            return method.MakeGenericMethod(insideType);
        }

        //static AssetRoot CreateRoot<T>(AssetID id)
        //{
        //    var assetRoot = new AssetRoot(id);
        //    AssetsBank.Add(id, assetRoot);
        //}

        //static T GetDefaultAssetContent<T>()
        //{

        //}
    }
}
