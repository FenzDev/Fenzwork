using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public record struct AssetID(string AssetName, Type AssetType);

    public static class AssetsManager
    {
        internal static Dictionary<AssetID, AssetRoot> _AssetsBank = [];

        public static void Init()
        {
            InternalInit(Assembly.GetCallingAssembly());
        }
        internal static void InternalInit(Assembly asm)
        {
            AssetsAssemblies.Add(SetupAssetsAssembly(FenzworkGame.LongName, asm, ""));
        }
            
        /// <summary>
        /// This must be called for when loading mods' Assets, this is also used internally for the actual game
        /// </summary>
        /// <param name="name">The mod/game assembly name (used for debugging).</param>
        /// <param name="assembly">The mod/game assembly where Assets were included with.</param>
        /// <param name="asmRelativeDirectory">The relative path from the main (game) assembly. e.g
        /// ""=(Game Assets) "Mods/CoolMod"=(CoolMod's Assets)</param>
        public static AssetsAssembly SetupAssetsAssembly(string name, Assembly assembly, string asmRelativeDirectory)
        {
            var registry = assembly.GetType("Fenzwork._AutoGen.AssetsRegistry");

            var dbgWorkingDir = (string)registry.GetField("DebugWorkingDirectory", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var assetsDirName = (string)registry.GetField("AssetsDirectoryName", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var registerMethod = registry.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic);

            var relDir = asmRelativeDirectory == ""? assetsDirName: Path.Combine(asmRelativeDirectory, assetsDirName);

            var assetsAsm = new AssetsAssembly() { Name = name, RelativeDir = relDir, WorkingDir = dbgWorkingDir };

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

        public static void Register<T>(string method, string assetName)
        {
            var assetRoot = GetRoot(assetName, typeof(T));
            
            _RegisteringAssetsAssembly.Roots.Add(assetRoot);

        }
    }

}
 