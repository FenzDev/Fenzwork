using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.GenLib.Models
{
    public class AssetsClassNode
    {
        public string Name { get; set; } = "";
        public string? AssetName { get; set; }
        public string? PropertyType { get; set; }
        public Dictionary<string, AssetsClassNode>? Members { get; set; }
    }
}
