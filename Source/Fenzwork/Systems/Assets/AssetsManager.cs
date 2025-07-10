using Fenzwork.Graphics;
using Fenzwork.Systems.Assets.Loaders;
using Fenzwork.Systems.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public record struct AssetID(string AssetName, Type AssetType)
    {
        public override readonly string ToString() => $"{AssetName} ({AssetType.Name})";
        public static implicit operator string(AssetID asset) => asset.ToString();
    }

    public static class AssetsManager
    {
        public static AssetsAutoLoadingWay AutoLoadingWay = AssetsAutoLoadingWay.Lazy;

        public static Dictionary<Type, RawAssetLoader> RawAssetLoaders = new()
        {
            [typeof(string)] = new TextLoader(),
            [typeof(AtlasMetadata)] = new AtlasMetadataLoader(),
            [typeof(GuiView)] = new GuiLoader(GuiAssetType.View)
        };
        internal static Dictionary<AssetID, AssetRoot> _AssetsBank = [];
        public static Dictionary<string, AssetRoot> DebugPaths = [];

        public static Action<AssetsAssembly> DebuggerInitMethod { get; set; } = null;
        public static Action DebuggerTickMethod { get; set; } = null;

        public static void Init()
        {
            InternalInit(Assembly.GetCallingAssembly());
        }
        internal static void InternalInit(Assembly asm)
        {
            SetupAssetsAssembly(FenzworkGame.LongName, 0, asm, "");
        }

        public static void Tick()
        {
            DebuggerTickMethod?.Invoke();
        }

        /// <summary>
        /// This must be called for when loading mods' Assets, this is also used internally for the actual game
        /// </summary>
        /// <param name="name">The mod/game assembly name (used for debugging).</param>
        /// <param name="indexOnList">Index on <see cref="AssetsManager.AssetsAssemblies"/> to know whether to load assets or an asm has more priority.</param>
        /// <param name="assembly">The mod/game assembly where Assets were included with.</param>
        /// <param name="asmRelativeDirectory">The relative path from the main (game) assembly. e.g
        /// ""=(Game RootsWithTheirMethod) "Mods/CoolMod"=(CoolMod's RootsWithTheirMethod)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Trimming", "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.", Justification = "<Pending>")]
        public static AssetsAssembly SetupAssetsAssembly(string name, int indexOnList, Assembly assembly, string asmRelativeDirectory)
        {
            var registry = assembly.GetType("Fenzwork._AutoGen.AssetsRegistry");

            var isDbg = (bool)registry.GetField("IsDebug", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var workingDir = (string)registry.GetField("WorkingDirectory", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var assetConfigFile = (string)registry.GetField("AssetsConfigFile", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var assetsDirName = (string)registry.GetField("AssetsDirectoryName", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var registerMethod = registry.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic);

            var relDir = asmRelativeDirectory == ""? assetsDirName: Path.Combine(asmRelativeDirectory, assetsDirName);

            var assetsAsm = new AssetsAssembly()
            {
                Name = name,
                AssetsDir = relDir,
                IsDebugging = isDbg,
                WorkingDirectory = workingDir,
                BuildOutputDirectory = isDbg? Path.Combine(workingDir, "bin", PlatformInfo.MonoGamePlatform.ToString(), ".mgcref") 
                                            : string.Empty,
                BuildIntermediateDirectory = isDbg? Path.Combine(workingDir, "obj", PlatformInfo.MonoGamePlatform.ToString(), "net8.0", ".mgcref")
                                            : string.Empty,
                AssetsConfigFile = assetConfigFile,
                TypesAssembly = assembly
            };

            if (isDbg)
                DebuggerInitMethod?.Invoke(assetsAsm);

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
            return GetRoot(assetName, typeof(T)).CreateAndAddAssetReference<T>();
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

        /// <summary>
        /// Do not use, this should be used by Assets class when setting up.
        /// </summary>
        public static void Register<T>(string method, string assetName, string assetDebugFullPath, object otherParam = null)
        {
            Register(_RegisteringAssetsAssembly, typeof(T), method, assetName, assetDebugFullPath, otherParam);
        }

        public static AssetRoot Register(AssetsAssembly asm, Type type, string method, string assetName, string assetDebugFullPath, object otherParam = null)
        {
            var assetRoot = GetRoot(assetName, type);

            if (method == "pack")
            {
                assetRoot.PresistantData ??= new SpritePresistantData();
                assetRoot.Param = Get<AtlasMetadata>((string)otherParam!);
            }

            asm.RootsWithTheirMethod.TryAdd(assetRoot, method);
            if (asm.IsDebugging)
            {
                DebugPaths.TryAdd(assetDebugFullPath, assetRoot);
            }

            var wasSourcless = false;
            var regestringAsmHasMorePriority = AssetsAssemblies.IndexOf(assetRoot.Source) < AssetsAssemblies.IndexOf(asm);
            if (assetRoot.Source == null || regestringAsmHasMorePriority)
            {
                wasSourcless = true;

                assetRoot.Source = asm;

                if (regestringAsmHasMorePriority && assetRoot.IsLoaded)
                    UnloadAsset(assetRoot);
            }

            if (assetRoot.AssetReferencesCount > 0 && (wasSourcless || !assetRoot.IsLoaded))
                LoadAsset(assetRoot, true);

            return assetRoot;
        }

        public static void Unregister(AssetRoot assetRoot, AssetsAssembly from)
        {
            if (!from.RootsWithTheirMethod.Remove(assetRoot))
                return;

            if (assetRoot.Source == from)
            {
                var asmIdx = AssetsAssemblies.IndexOf(from);
                AssetsAssembly source;
                do
                {
                    if (asmIdx == 0)
                    {
                        assetRoot.Source = null;
                        // we wont unload, we want to keep the previous content.
                        return;
                    }

                    source = AssetsAssemblies[--asmIdx];

                } while (!source.RootsWithTheirMethod.ContainsKey(assetRoot));

                if (assetRoot.IsLoaded)
                    UnloadAsset(assetRoot);

                assetRoot.Source = source;
                
                if (assetRoot.IsLoaded)
                    LoadAsset(assetRoot);

            }
        }

        internal static void UnloadAsset(AssetRoot assetRoot)
        {
            if (assetRoot.Source is null || !assetRoot.IsLoaded)
                return;

            if (assetRoot.Content is IDataEmbededAsset dataEmbededAsset)
                assetRoot.PresistantData = dataEmbededAsset.PresistantData;

            var method = assetRoot.Source.RootsWithTheirMethod[assetRoot];

            if (method.Equals("build"))
                UnloadBuildAsset(assetRoot);
            else if (method.Equals("copy"))
                UnloadCopyAsset(assetRoot);
            else if (method.Equals("pack"))
                UnloadSpriteAsset(assetRoot);

        }

        internal static void LoadAsset(AssetRoot assetRoot, bool forceLoad = false)
        {
            if (!forceLoad && (assetRoot.Source is null || assetRoot.IsLoaded))
                return;

            var method = assetRoot.Source.RootsWithTheirMethod[assetRoot];

            if (method.Equals("build"))
                LoadBuildAsset(assetRoot);
            else if (method.Equals("copy"))
                LoadCopyAsset(assetRoot);
            else if (method.Equals("pack"))
                LoadSpriteAsset(assetRoot);
            
            if (assetRoot.Content is IDataEmbededAsset dataEmbededAsset)
            {
                dataEmbededAsset.PresistantData = assetRoot.PresistantData;
            }
        }

        internal static void LoadSpriteAsset(AssetRoot assetRoot)
        {
            var sprite = new Sprite() { _Metadata = (Asset<AtlasMetadata>)assetRoot.Param };
            var spriteMetadata = sprite._Metadata.Content.Sprites[assetRoot];
            sprite.Texture = sprite._Metadata.Content.Atlases[spriteMetadata.AtlasId].CreateAndAddAssetReference<Texture2D>();
            sprite.SourceRect = new(spriteMetadata.X, spriteMetadata.Y, spriteMetadata.Width, spriteMetadata.Height);

            assetRoot.Content = sprite;
            assetRoot.IsLoaded = true;
        }
        internal static void UnloadSpriteAsset(AssetRoot assetRoot)
        {
            ((Sprite)assetRoot.Content!).Texture.Dispose();

            assetRoot.IsLoaded = false;
            assetRoot.Content = null;
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
            if (assetRoot.Source.IsDebugging)
                fileStream = File.OpenRead(Path.Combine(assetRoot.Source.WorkingDirectory, assetRoot.ID.AssetName));
            else
                fileStream = TitleContainer.OpenStream(Path.Combine(assetRoot.Source.AssetsDir, assetRoot.ID.AssetName));

            var assetLoader = RawAssetLoaders[assetRoot.ID.AssetType];
            assetLoader.DoLoad(fileStream, assetRoot.ID, out var content);
            fileStream.Close();

            assetRoot.Content = content;
            assetRoot.IsLoaded = true;

        }

        internal static void UnloadBuildAsset(AssetRoot assetRoot)
        {
            var assetExtlessName = $"{Path.GetDirectoryName(assetRoot.ID.AssetName)}/{Path.GetFileNameWithoutExtension(assetRoot.ID.AssetName)}";

            string dir;
            if (assetRoot.Source.IsDebugging)
                dir = Path.Combine(assetRoot.Source.WorkingDirectory, "bin", PlatformInfo.MonoGamePlatform.ToString(), ".mgcref");
            else
                dir = assetRoot.Source.AssetsDir;

            var oldDir = FenzworkGame._GameSingleton.Content.RootDirectory;
            FenzworkGame._GameSingleton.Content.RootDirectory = dir;
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

            string dir;
            if (assetRoot.Source.IsDebugging)
                dir = Path.Combine(assetRoot.Source.WorkingDirectory, "bin", PlatformInfo.MonoGamePlatform.ToString(), ".mgcref");
            else
                dir = assetRoot.Source.AssetsDir;

            var assetExtlessName = $"{Path.GetDirectoryName(assetRoot.ID.AssetName)}/{Path.GetFileNameWithoutExtension(assetRoot.ID.AssetName)}";

            var genericMethod = LoadMethod.MakeGenericMethod(assetRoot.ID.AssetType);
            var oldDir = FenzworkGame._GameSingleton.Content.RootDirectory;
            FenzworkGame._GameSingleton.Content.RootDirectory = dir;
            var returnedContent = genericMethod.Invoke(FenzworkGame._GameSingleton.Content, [assetExtlessName]);
            FenzworkGame._GameSingleton.Content.RootDirectory = oldDir;

            assetRoot.Content = returnedContent;
            assetRoot.IsLoaded = true;
        }
    }
}
 