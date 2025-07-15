using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public sealed class AssetRoot
    {
        internal AssetRoot() { OnLoaded += DummyMethod; OnUnloading += DummyMethod; }

        public AssetID ID { get; init; }
        public object? Content { get; internal set; }
        public bool IsLoaded { get; internal set; }
        public bool SupressUnloading { get; internal set; }
        public object? Param { get; internal set; }
        /// <summary>
        /// This is for when you need to store data that presists loading/unloading process unlike Content.
        /// </summary>
        public object? PresistantData { get; internal set; }
        public AssetsAssembly Source { get; internal set; }
        
        private List<WeakReference> _AssetReferences = [];
        public int AssetReferencesCount => _AssetReferences.Count;
        
        internal Asset<T> CreateAndAddAssetReference<T>()
        {
            var asset = new Asset<T>(this);

            for (int i = 0; i < _AssetReferences.Count; i++)
                if (!_AssetReferences[i].IsAlive)
                    _AssetReferences.RemoveAt(i--);

            _AssetReferences.Add(new WeakReference(_AssetReferences));

            if (Source != null && !IsLoaded)
                AssetsManager.LoadAsset(this);

            return asset;
        }
        internal void RemoveAssetReference<T>(Asset<T> asset)
        {
            for (int i = 0; i < AssetReferencesCount; i++)
                // if a reference is not alive out of the list and we didn't notice we remove it aswell
                if (_AssetReferences[i].IsAlive || _AssetReferences[i].Target == asset)
                    _AssetReferences.RemoveAt(i--);
        }

        public override string ToString() => ID.ToString();

        internal void DummyMethod() {}
        internal void InvokeOnLoaded() => OnLoaded();
        internal void InvokeOnUnloading() => OnUnloading();
        public event Action OnLoaded;
        public event Action OnUnloading;
    }

}
