using Fenzwork.AssetsLibrary.Models;
using Fenzwork.Services;
using Fenzwork.Systems.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork
{
    public static class AssetsManager
    {
        public static AssetLoadingTime LoadingTime { get; internal set; } = AssetLoadingTime.Lazy;

        //internal readonly static IEnumerable<string> DebugWorkingDirectories = new List<string>();

        internal static Dictionary<AssetID, AssetRoot> AssetsBank = new();

        public static AssetHandle<T> Get<T>(string assetname, string domain = "") => Get<T>(new (assetname, domain, typeof(T)));
        public static AssetHandle<T> Get<T>(AssetID id)
        {
            return GetRoot(id).OpenHandle<T>();
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
            
            if (LoadingTime == AssetLoadingTime.Lazy)
                LoadAsset(assetRoot);
        }

        internal static void LoadAsset(AssetRoot asset)
        {
            // TODO: Loading
        }

        //static AssetRoot CreateRoot<T>(AssetID id)
        //{
        //    var root = new AssetRoot(id);
        //    AssetsBank.Add(id, root);
        //}

        //static T GetDefaultAssetContent<T>()
        //{

        //}
    }
}
