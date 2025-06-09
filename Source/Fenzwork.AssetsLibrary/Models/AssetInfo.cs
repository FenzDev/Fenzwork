using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.AssetsLibrary.Models
{
    public struct AssetInfo
    {
        public AssetInfo() { }

        public AssetInfo(string infoStr)
        {
            var assetInfoSplited = infoStr.Split(':');
            
            Method = assetInfoSplited[0];
            Type = assetInfoSplited[1];
            AssetsPath = assetInfoSplited[2];
            AssetPath = assetInfoSplited[3];
            AssetName = assetInfoSplited[4];
            Domain = assetInfoSplited[5];
            Parameter = assetInfoSplited[6];
        }

        public string Method;
        public string Type;
        public string AssetName;
        public string Domain;
        public string Parameter;
        public string AssetsPath;
        public string AssetPath;

        public override string ToString() => $"{Method}:{Type}:{AssetsPath}:{AssetPath}:{AssetName}:{Domain}:{Parameter}";
    }
}
