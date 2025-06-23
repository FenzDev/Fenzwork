using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public class AssetsAssembly
    {
        public string Name { get; init; }
        public Assembly TypesAssembly { get; init; }
        public string AssetsDir { get; init; }
        public bool IsDebugging { get; init; }
        public string AssetsConfigFile { get; init; }
        public string WorkingDirectory { get; init; }
        /// <summary>
        /// Dictionary of AssetRoot along with the loading method.
        /// </summary>
        public Dictionary<AssetRoot, string> RootsWithTheirMethod = [];

        public override string ToString() => $"AssetsAssembly ({Name})";
        
    }
}
