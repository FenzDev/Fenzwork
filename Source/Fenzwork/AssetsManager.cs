using Fenzwork.AssetsLibrary.Models;
using Fenzwork.Services;
using Fenzwork.Systems.Assets;
using Fenzwork.Systems.Assets.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork
{
    public static class AssetsManager
    {
        static AssetsManager()
        {
            CustomLoaders.Add(typeof(object), new JsonLoader());
            CustomLoaders.Add(typeof(string), new TextLoader());
        }

        public static string AssemblyRelativeDirectory = "";
        public static AssetLoadingTime LoadingTime { get; internal set; } = AssetLoadingTime.Lazy;

        internal readonly static IEnumerable<(string ConfigPath, string AssetsPath)> DebugWorkingDirectories = new List<(string ConfigPath, string AssetsPath)>();

        internal static Dictionary<AssetID, AssetRoot> AssetsBank { get; } = new();
        public static Dictionary<Type, AssetCustomLoader> CustomLoaders { get; } = new();


        public static AssetHandle<T> Get<T>(string assetname, string domain = "") => Get<T>(new (assetname, domain, typeof(T)));
        public static AssetHandle<T> Get<T>(AssetID id)
        {
            var assetRoot = GetRoot(id);

            if (!assetRoot.IsLoaded && !assetRoot.IsRegistered && LoadingTime == AssetLoadingTime.Lazy)
                LoadAsset(assetRoot);

            return assetRoot.OpenHandle<T>();
        }
        static AssetRoot GetRoot(AssetID id)
        {
            if (AssetsBank.TryGetValue(id, out var root))
            {
                return root;
            }
            else
            {
                var newRoot = new AssetRoot(id);
                
                AssetsBank.Add(id, new AssetRoot(id));

                return newRoot;
            }
        }

        internal static void RegisterAsset(AssetInfo info)
        {
            var assetID = new AssetID(info.Domain, info.AssetName, Type.GetType(info.Type));
            var assetRoot = GetRoot(assetID);

            assetRoot.FullInfo = info;
            assetRoot.IsRegistered = true;
        }

        internal static void LoadAsset(AssetRoot assetRoot)
        {
            if (assetRoot.IsLoaded || !assetRoot.IsRegistered) return;

            switch (assetRoot.FullInfo.Method)
            {
                case "build":
                    LoadBuildAsset(assetRoot);
                    break;
                case "copy":
                    LoadCopyAsset(assetRoot);
                    break;
            }
        }

        internal static void LoadCopyAsset(AssetRoot assetRoot)
        {
            var assetPath = AssemblyRelativeDirectory == null ? Path.Combine(assetRoot.FullInfo.AssetsPath, assetRoot.FullInfo.AssetPath) : Path.Combine(AssemblyRelativeDirectory, assetRoot.FullInfo.AssetsPath, assetRoot.FullInfo.AssetPath);
            using var stream = TitleContainer.OpenStream(assetPath);
            try
            {
                if (CustomLoaders.TryGetValue(assetRoot.ID.AssetType, out var loader))
                {
                    loader.DoLoad(stream, assetRoot.ID, assetRoot.FullInfo.Parameter, out var assetContent);

                    assetRoot.Content = assetContent;
                    assetRoot.IsLoaded = true;
                }
            }
            catch { }

        }

        internal static void LoadBuildAsset(AssetRoot assetRoot)
        {
            var dir = AssemblyRelativeDirectory == ""? assetRoot.FullInfo.AssetsPath: Path.Combine(AssemblyRelativeDirectory, assetRoot.FullInfo.AssetsPath);

            var oldDir = MGGame.Instance.Content.RootDirectory;
            MGGame.Instance.Content.RootDirectory = dir;

            assetRoot.Content = GetGenericMethod(typeof(ContentManager), "Load", assetRoot.ID.AssetType).Invoke(MGGame.Instance.Content, [ assetRoot.FullInfo.AssetPath ]);


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
