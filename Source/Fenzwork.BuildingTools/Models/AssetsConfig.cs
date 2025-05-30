using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fenzwork.BuildingTools.Models
{
    internal class AssetsConfig
    {
        [JsonInclude]
        public string Profile;
        [JsonInclude]
        public string Compress;
        [JsonInclude]
        public string[] References = [];
        [JsonInclude]
        public Configuration[] Configurations = [];
    }
}
