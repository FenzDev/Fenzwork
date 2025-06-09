using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fenzwork.AssetsLibrary.Models
{
    public class MainConfig
    {
        [JsonInclude]
        public bool EnableDomains = false;
        [JsonInclude]
        public string Profile = "";
        [JsonInclude]
        public bool Compress = false;
        [JsonInclude]
        public string[] References = [];
        [JsonInclude]
        public GroupConfig[] Configurations = [];
    }
}
