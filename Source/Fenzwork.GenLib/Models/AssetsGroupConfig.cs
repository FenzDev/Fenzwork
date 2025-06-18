using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fenzwork.GenLib.Models
{
    public class AssetsGroupConfig
    {
        [JsonInclude]
        public string From = "";
        [JsonInclude]
        public string[] Include = [];
        [JsonInclude]
        public string Method = "";
        [JsonInclude]
        public string LoadAs = "";
        [JsonInclude]
        public string BuildImporter = "";
        [JsonInclude]
        public string BuildProcessor = "";
        [JsonInclude]
        public string[] BuildProcessorParams = [];
    }
}
