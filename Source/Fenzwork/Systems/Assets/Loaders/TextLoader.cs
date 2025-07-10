using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets.Loaders
{
    public class TextLoader : RawAssetLoader
    {
        protected override void Load(Stream stream, AssetID assetID, out object resultAsset)
        {
            var reader = new StreamReader(stream);
            resultAsset = reader.ReadToEnd();
        }
    }
}
