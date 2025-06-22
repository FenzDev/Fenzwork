using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public class AssetsAssembly
    {
        public string Name { get; init; }
        public string AssetsDir { get; init; }
        public bool IsDebugging { get; init; }
        /// <summary>
        /// Dictionary of AssetRoot along with the loading method.
        /// </summary>
        public Dictionary<AssetRoot, string> RootsWithLoadMethod = [];

        public override string ToString() => $"AssetsAssembly ({Name})";
        
    }
}
