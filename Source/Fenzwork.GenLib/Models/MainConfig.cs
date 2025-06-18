using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fenzwork.GenLib.Models
{
    public class MainConfig
    {
        [JsonInclude]
        public string AssetsDirectoryName = "Assets";
        [JsonInclude]
        public bool EnableDomainFolders = false;
        [JsonInclude]
        public string BuildPlatform = "DesktopGL";
        [JsonInclude]
        public string BuildProfile = "";
        [JsonInclude]
        public bool BuildCompress = false;
        [JsonInclude]
        public string[] BuildReferences = [ ];
        [JsonInclude]
        public AssetsGroupConfig[] Assets = [];
    }
}
