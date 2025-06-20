using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public sealed class AssetRoot : IDisposable
    {
        internal AssetRoot() { }

        public AssetID ID { get; init; }
        public object Content { get; }
        
        private int _AssetReferencesCounter;
        public int AssetReferencesCounter => _AssetReferencesCounter;
        public AssetsAssembly Source { get; init; }
        
        internal Asset<T> CreateAndIncrementAssetReference<T>()
        {
            _AssetReferencesCounter++;
            return new Asset<T>(this);
        }
        internal void DecrementAssetReference()
        {
            _AssetReferencesCounter--;
        }

        public void Dispose()
        {
        }
    }

}
