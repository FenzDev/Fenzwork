using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fenzwork.AssetsLibrary.Models
{
    public class GroupConfig
    {
        [JsonInclude]
        public string BaseFolder = "";
        [JsonInclude]
        public string[] Include = [];
        [JsonInclude]
        public bool RemovesTrailingExtensions = true;
        [JsonInclude]
        public string Method = "";
        [JsonInclude]
        public string Type = "";
        [JsonInclude] 
        public string[] Properties = [];

    }
}
