using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public interface IAssetHandle
    {
        public AssetID ID { get; }
        public bool IsLoaded { get; internal set; }
        public object Content { get; internal set; }
    }
}
