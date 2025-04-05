using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenzExt.AssetsSystem
{
    public class DynamicAsset<T> : IDynamicAsset
    {
        public AssetID ID { get; internal set; }
        public bool IsLoaded { get; internal set; }
        public T Content { get; internal set; }
        AssetID IDynamicAsset.ID => ID;
        bool IDynamicAsset.IsLoaded { get => IsLoaded; set => IsLoaded = value; }
        object IDynamicAsset.Content { get => Content; set => Content = (T)value; }

        public override string ToString() => ID.ToString();
    }
}
