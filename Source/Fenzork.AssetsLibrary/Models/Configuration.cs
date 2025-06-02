using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fenzwork.AssetsLibrary.Models
{
    public class Configuration
    {
        [JsonInclude]
        public string[] Include = [];
        [JsonInclude]
        public string Type;
        [JsonInclude] 
        public string[] Properties = [];
        [JsonInclude] 
        public Configuration[] Exceptions = [];

        public override int GetHashCode() => HashCode.Combine(Include, Type, Properties);
    }
}
