using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{

    public abstract class AssetRawLoader
    {
        protected abstract void Load(Stream stream, AssetID assetID, out object resultAsset);
        internal void DoLoad(Stream stream, AssetID assetID, out object resultAsset) => Load(stream, assetID, out resultAsset);

        protected virtual void Unload(AssetID assetID, object oldAsset) { }
        internal void DoUnload(AssetID assetID, object oldAsset) => Unload(assetID, oldAsset);

    }
}
