using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    /// <summary>
    /// Asset handle, you should store this (instead of the content) inside of you system class for example.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Asset<T> : IDisposable
    {
        internal Asset(AssetRoot root)
        {
            _Root = root;
        }

        private AssetRoot _Root;
        public AssetRoot Root => _Root;
        public AssetID ID => _Root.ID;
        public T Content => (T)_Root.Content!;

        public override string ToString() => _Root.ID.ToString();

        public static implicit operator T(Asset<T> asset) => (T)asset.Content;

        public void Dispose() => Dispose(true);

        void Dispose(bool disposed)
        {
            if (disposed)
            {
                Root.RemoveAssetReference(this);
                GC.SuppressFinalize(this);
            }
        }
        
        ~Asset()
        {
            Dispose(false);
        }
    }
}
