using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    internal interface IAssetHandle
    {
        public AssetID ID { get; }
        public bool IsLoaded { get; internal set; }
        public object Content { get; internal set; }
    }
}
