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
        public string WorkingDir { get; init; }
        public string RelativeDir { get; init; }
        public List<AssetRoot> Roots = [];

        public override string ToString() => $"AssetsAssembly ({Name})";
    }
}
