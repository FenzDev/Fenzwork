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
        internal Asset(string untrackedId, T untrackedContent) 
        {
            _UntrackedID = new (untrackedId, typeof(T));
            _UntrackedContent = untrackedContent;
        }
        internal Asset(AssetRoot root)
        {
            _Root = root;
            _Root.OnLoaded += OnLoaded;
            _Root.OnUnloading += OnUnloading;
        }

        private readonly AssetRoot _Root;
        public AssetRoot Root => _Root;
        private readonly AssetID _UntrackedID;
        public AssetID ID => _Root == null ? _UntrackedID : _Root.ID;
        private readonly T _UntrackedContent; 
        public T Content => _Root == null ? _UntrackedContent: (T)_Root.Content!;

        public override string ToString() => ID.ToString();

        public override int GetHashCode() => Content?.GetHashCode() ?? 0;

        public static implicit operator T(Asset<T> asset) => asset.Content;

        public event Action OnLoaded;
        public event Action OnUnloading;

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
