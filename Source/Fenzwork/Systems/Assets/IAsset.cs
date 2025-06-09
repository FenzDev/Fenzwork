using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public interface IAsset
    {
        AssetID ID { get; }
        bool IsLoaded { get;  }
        object Content { get; }
    }
}
