using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public enum AssetsAutoLoadingWay
    {
        /// <summary>
        /// That means the assets will be loaded when needed and unloaded when not. This is useful for complex games or when debugging
        /// </summary>
        Lazy,
        /// <summary>
        /// That means the assets will be loaded on start and won't be unloaded at all. This is useful for lightweight games release
        /// </summary>
        Eager,
    }
}
