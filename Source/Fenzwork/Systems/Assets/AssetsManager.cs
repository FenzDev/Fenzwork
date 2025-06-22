using Fenzwork.Systems.Assets.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public record struct AssetID(string AssetName, Type AssetType);

    public static class AssetsManager
    {
        public static AssetsAutoLoadingWay AutoLoadingWay = AssetsAutoLoadingWay.Lazy;

        public static Dictionary<Type, AssetRawLoader> RawAssetLoaders = new() { {typeof(string), new TextLoader()} };
        internal static Dictionary<AssetID, AssetRoot> _AssetsBank = [];
        public static Dictionary<string, AssetRoot> DebugPaths = [];

        public static void Init()
        {
            InternalInit(Assembly.GetCallingAssembly());
        }
        internal static void InternalInit(Assembly asm)
        {
            SetupAssetsAssembly(FenzworkGame.LongName, 0, asm, "");
        }
            
        /// <summary>
        /// This must be called for when loading mods' Assets, this is also used internally for the actual game
        /// </summary>
        /// <param name="name">The mod/game assembly name (used for debugging).</param>
        /// <param name="indexOnList">Index on <see cref="AssetsManager.AssetsAssemblies"/> to know whether to load assets or an asm has more priority.</param>
        /// <param name="assembly">The mod/game assembly where Assets were included with.</param>
        /// <param name="asmRelativeDirectory">The relative path from the main (game) assembly. e.g
        /// ""=(Game RootsWithLoadMethod) "Mods/CoolMod"=(CoolMod's RootsWithLoadMethod)</param>
        public static AssetsAssembly SetupAssetsAssembly(string name, int indexOnList, Assembly assembly, string asmRelativeDirectory)
        {
            var registry = assembly.GetType("Fenzwork._AutoGen.AssetsRegistry");

            var isDbg = (bool)registry.GetField("IsDebug", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var assetsDirName = (string)registry.GetField("AssetsDirectoryName", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var registerMethod = registry.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic);

            var relDir = asmRelativeDirectory == ""? assetsDirName: Path.Combine(asmRelativeDirectory, assetsDirName);

            var assetsAsm = new AssetsAssembly() { Name = name, AssetsDir = relDir, IsDebugging = isDbg };

            AssetsAssemblies.Insert(indexOnList, assetsAsm);

            _RegisteringAssetsAssembly = assetsAsm;
            registerMethod.Invoke(null, []);
            _RegisteringAssetsAssembly = null;

            return assetsAsm;
        }


        private static AssetsAssembly _RegisteringAssetsAssembly;

        /// <summary>
        /// This should be ordered for when they overrwrite other assets.
        /// </summary>
        public static List<AssetsAssembly> AssetsAssemblies = [ ];

        public static Asset<T> Get<T>(string assetName)
        {
            return GetRoot(assetName, typeof(T)).CreateAndIncrementAssetReference<T>();
        }
        
        internal static AssetRoot GetRoot(string assetName, Type type)
        {
            var assetId = new AssetID(assetName, type);
            
            if (_AssetsBank.TryGetValue(assetId, out var assetRoot))
                return assetRoot;

            var newAssetRoot = new AssetRoot() { ID = assetId };
            _AssetsBank.Add(assetId, newAssetRoot);
            return newAssetRoot;
        }

        public static void Register<T>(string method, string assetName, string assetDebugFullPath)
        {
            var assetRoot = GetRoot(assetName, typeof(T));

            _RegisteringAssetsAssembly.RootsWithLoadMethod.Add(assetRoot, method);
            if (_RegisteringAssetsAssembly.IsDebugging)
            {
                DebugPaths.TryAdd(assetDebugFullPath, assetRoot);
            }

            var regestringAsmHasMorePriority = AssetsAssemblies.IndexOf(assetRoot.Source) < AssetsAssemblies.IndexOf(_RegisteringAssetsAssembly);
            if (assetRoot.Source == null || regestringAsmHasMorePriority)
            {
                if (regestringAsmHasMorePriority && !assetRoot.IsLoaded)
                    UnloadAsset(assetRoot);
                    
                assetRoot.Source = _RegisteringAssetsAssembly;
            }

            if (assetRoot.AssetReferencesCounter > 0 && !assetRoot.IsLoaded)
                LoadAsset(assetRoot);

        }

        internal static void UnloadAsset(AssetRoot assetRoot)
        {
            if (assetRoot.Source is null || !assetRoot.IsLoaded)
                return;

            var method = assetRoot.Source.RootsWithLoadMethod[assetRoot];

            if (method.Equals("build"))
                UnloadBuildAsset(assetRoot);
            else if (method.Equals("copy"))
                UnloadCopyAsset(assetRoot);

        }

        internal static void LoadAsset(AssetRoot assetRoot)
        {
            if (assetRoot.Source is null || assetRoot.IsLoaded)
                return;

            var method = assetRoot.Source.RootsWithLoadMethod[assetRoot];

            if (method.Equals("build"))
                LoadBuildAsset(assetRoot);
            else if (method.Equals("copy"))
                LoadCopyAsset(assetRoot);

        }

        internal static void UnloadCopyAsset(AssetRoot assetRoot)
        {
            var assetLoader = RawAssetLoaders[assetRoot.ID.AssetType];
            assetLoader.DoUnload(assetRoot.ID, assetRoot.Content);

            assetRoot.IsLoaded = false;
            assetRoot.Content = null;
        }
        internal static void LoadCopyAsset(AssetRoot assetRoot)
        {
            Stream fileStream;
            if (Path.IsPathRooted(assetRoot.Source.AssetsDir))
                fileStream = File.OpenRead(Path.Combine(assetRoot.Source.AssetsDir, assetRoot.ID.AssetName));
            else
                fileStream = TitleContainer.OpenStream(Path.Combine(assetRoot.Source.AssetsDir, assetRoot.ID.AssetName));

            var assetLoader = RawAssetLoaders[assetRoot.ID.AssetType];
            assetLoader.DoLoad(fileStream, assetRoot.ID, out var content);

            assetRoot.Content = content;
            assetRoot.IsLoaded = true;

        }

        internal static void UnloadBuildAsset(AssetRoot assetRoot)
        {
            var assetExtlessName = $"{Path.GetDirectoryName(assetRoot.ID.AssetName)}/{Path.GetFileNameWithoutExtension(assetRoot.ID.AssetName)}";

            var oldDir = FenzworkGame._GameSingleton.Content.RootDirectory;
            FenzworkGame._GameSingleton.Content.RootDirectory = assetRoot.Source.AssetsDir;
            FenzworkGame._GameSingleton.Content.UnloadAsset(assetExtlessName);
            FenzworkGame._GameSingleton.Content.RootDirectory = oldDir;

            assetRoot.IsLoaded = false;
            assetRoot.Content = null;
        }

        internal static MethodInfo LoadMethod;
        internal static void LoadBuildAsset(AssetRoot assetRoot)
        {
            if (LoadMethod == null)
                LoadMethod = typeof(ContentManager).GetMethod("Load");

            var assetExtlessName = $"{Path.GetDirectoryName(assetRoot.ID.AssetName)}/{Path.GetFileNameWithoutExtension(assetRoot.ID.AssetName)}";

            var genericMethod = LoadMethod.MakeGenericMethod(assetRoot.ID.AssetType);
            var oldDir = FenzworkGame._GameSingleton.Content.RootDirectory;
            FenzworkGame._GameSingleton.Content.RootDirectory = assetRoot.Source.AssetsDir;
            var returnedContent = genericMethod.Invoke(FenzworkGame._GameSingleton.Content, [assetExtlessName]);
            FenzworkGame._GameSingleton.Content.RootDirectory = oldDir;

            assetRoot.Content = returnedContent;
            assetRoot.IsLoaded = true;
        }
    }
}
 