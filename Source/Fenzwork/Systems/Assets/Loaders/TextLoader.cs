using System.Collections.Immutable;
using System.IO;
using Fenzwork.AssetsLibrary.Models;
using Fenzwork.Systems.Assets;
using Microsoft.VisualBasic;

namespace Fenzwork.Systems.Assets.Loaders
{
    internal class TextLoader : AssetCustomLoader
    {
        protected override void Load(Stream stream, AssetID assetID, string suffixParameter, out object resultAsset)
        {
            using var reader = new StreamReader(stream);
            resultAsset = reader.ReadToEnd();
        }

        protected override bool Reload(Stream stream, AssetID assetID, string suffixParameter, object oldAsset, out object resultAsset)
        {
            DoLoad(stream, assetID, suffixParameter, out resultAsset);
            return true;
        }
    }
}
