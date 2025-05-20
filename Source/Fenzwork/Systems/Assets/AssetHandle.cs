using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public class AssetHandle<T> : IAssetHandle
    {
        public AssetID ID { get; internal set; }
        public bool IsLoaded { get; internal set; }
        public T Content { get; internal set; }
        AssetID IAssetHandle.ID => ID;
        bool IAssetHandle.IsLoaded { get => IsLoaded; set => IsLoaded = value; }
        object IAssetHandle.Content { get => Content; set => Content = (T)value; }

        public override string ToString() => ID.ToString();

        public static implicit operator T(AssetHandle<T> handle) => handle.Content;
    }
}
