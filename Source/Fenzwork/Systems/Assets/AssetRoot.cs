using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public sealed class AssetRoot
    {
        internal AssetRoot() { }

        public AssetID ID { get; init; }
        public object Content { get; internal set; }
        public bool IsLoaded { get; internal set; }
        
        private int _AssetReferencesCounter;
        public int AssetReferencesCounter => _AssetReferencesCounter;
        public AssetsAssembly Source { get; internal set; }
        
        internal Asset<T> CreateAndIncrementAssetReference<T>()
        {
            if (_AssetReferencesCounter++ == 0 && Source != null && !IsLoaded)
                AssetsManager.LoadAsset(this);

            return new Asset<T>(this);
        }
        internal void DecrementAssetReference()
        {
            _AssetReferencesCounter--;

            if (_AssetReferencesCounter == 0 && IsLoaded && AssetsManager.AutoLoadingWay == AssetsAutoLoadingWay.Lazy)
                AssetsManager.UnloadAsset(this);

        }

        public override string ToString() => ID.ToString();
    }

}
