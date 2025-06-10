using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using Fenzwork.AssetsLibrary.Models;
using Fenzwork.Systems.Assets;
using Microsoft.VisualBasic;

namespace Fenzwork.Systems.Assets.Loaders
{
    internal class JsonLoader : AssetCustomLoader
    {
        protected override void Load(Stream stream, AssetID assetID, string suffixParameter, out object resultAsset)
        {
            resultAsset = JsonSerializer.Deserialize(stream, assetID.AssetType);
        }

        protected override bool Reload(Stream stream, AssetID assetID, string suffixParameter, object oldAsset, out object resultAsset)
        {
            Load(stream, assetID, suffixParameter, out resultAsset);
            
            if (resultAsset != null)
                return true;

            return false;
        }
    }
}
