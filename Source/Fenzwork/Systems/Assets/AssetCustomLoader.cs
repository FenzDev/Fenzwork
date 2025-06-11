using Fenzwork.AssetsLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public abstract class AssetCustomLoader
    {
        /// <summary>
        /// Called when newly loaded.
        /// </summary>
        protected abstract void Load(Stream stream, AssetID assetID, string suffixParameter, out object resultAsset);
        internal void DoLoad(Stream stream, AssetID assetID, string suffixParameter, out object resultAsset) => Load(stream, assetID, suffixParameter, out resultAsset); 

        /// <summary>
        /// Called when overloading assets from different source or hot-reloaded.
        /// </summary>
        protected abstract bool Reload(Stream stream, AssetID assetID, string suffixParameter, object oldAsset, out object resultAsset);
        internal bool DoReload(Stream stream, AssetID assetID, string suffixParameter, object oldAsset, out object resultAsset) => Reload(stream, assetID, suffixParameter, oldAsset, out resultAsset);
        
        /// <summary>
        /// Called when overloading assets from different source or hot-reloaded.
        /// </summary>
        protected virtual void Unload(AssetID assetID, string suffixParameter, object oldAsset) { }
        internal void DoUnLoad(AssetID assetID, string suffixParameter, object oldAsset) => Unload(assetID, suffixParameter, oldAsset);

    }
}
