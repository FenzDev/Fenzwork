using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public sealed class AssetHandle<T> : IAsset, IDisposable
    {
        internal AssetHandle(AssetRoot root)
        {
            Root = root;
        }

        public AssetRoot Root { get; private set; }

        public AssetID ID => Root.ID;

        public bool IsLoaded => Root.IsLoaded;

        public T Content => (T)Root.Content;

        object IAsset.Content => Content;

        /// <summary>
        /// This will close the handle
        /// </summary>
        public void Dispose()
        {
            Root.CloseHandle();
        }

        public override string ToString() => ID.ToString();

        public static implicit operator T(AssetHandle<T> handle) => (T)handle.Content;
    }
}
