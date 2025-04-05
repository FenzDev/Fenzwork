using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenzExt.AssetsSystem
{
    internal interface IDynamicAsset
    {
        public AssetID ID { get; }
        public bool IsLoaded { get; internal set; }
        public object Content { get; internal set; }
    }
}
