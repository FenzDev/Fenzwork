using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public sealed class Asset<T> : IDisposable
    {
        internal Asset(AssetRoot root)
        {
            _Root = root;
        }

        private AssetRoot _Root;
        public AssetRoot Root => _Root;
        public T Content => (T)_Root.Content;

        public static implicit operator T(Asset<T> asset) => (T)asset.Content;

        public void Dispose()
        {
            _Root.DecrementAssetReference();
        }
    }
}
