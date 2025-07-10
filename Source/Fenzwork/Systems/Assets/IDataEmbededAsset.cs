using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public interface IDataEmbededAsset
    {
        /// <summary>
        /// Property which stores data that will remain through reloading of the asset.
        /// </summary>
        object PresistantData { get; set; }

    }
}
