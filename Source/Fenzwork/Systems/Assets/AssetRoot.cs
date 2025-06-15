using Fenzwork.AssetsLibrary.Models;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    /// <summary>
    /// Represents the root of all handles. There is one per Asset.
    /// </summary>
    public class AssetRoot : IAsset, IDisposable
    {

        internal AssetRoot(AssetID assetID)
        {
            ID = assetID;
        }

        public int NumberOfActiveHandles { get; private set; } = 0;

        public AssetID ID { get; private set; }
        internal List<AssetSourceInfo> Sources = new();
        internal AssetSourceInfo CurrentSource => IsRegistered? Sources[Sources.Count - 1]: default;
        public bool IsRegistered => Sources.Count > 0;
        public bool IsLoaded { get; internal set; }
        public AssetCustomLoader? Loader { get; internal set; }
        public object Content { get; internal set; }

        public void Dispose()
        {
            AssetsManager.AssetsBank.Remove(ID);

            if (Content is IDisposable content)
            {
                content.Dispose();
            }
        }

        internal AssetHandle<T> OpenHandle<T>()
        {
            var handle = new AssetHandle<T>(this);
            NumberOfActiveHandles++;

            if (AssetsManager.LoadingTime == AssetLoadingTime.Lazy)
                AssetsManager.LoadAsset(this);

            return handle;
        }
        internal void CloseHandle()
        {
            NumberOfActiveHandles--;
            
            if (NumberOfActiveHandles <= 0 && AssetsManager.LoadingTime == AssetLoadingTime.Lazy)
                AssetsManager.UnloadAsset(this);
        }
    }

    public record struct AssetSourceInfo( string AssemblyRelativeLocation, AssetInfo Info, bool IsWorkingDir = false);
}
