using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fenzwork.GenLib.Models
{
    public class AtlasConfig
    {
        [JsonInclude]
        public uint MaxTextureSize = 4096;
        [JsonInclude]
        public int SpritePadding = 2;
        [JsonInclude]
        public string AtlasNamePrefix = "TextureMap_";
        [JsonInclude]
        public string MetadataName = "TextureMap.metadata";
        [JsonInclude]
        public string Folder = "";
    }

}
