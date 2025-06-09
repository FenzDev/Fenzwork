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
        public AssetInfo FullInfo { get; internal set; }
        public bool IsRegistered { get; internal set; }
        public bool IsLoaded { get; internal set; }
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
            return handle;
        }
        internal void CloseHandle()
        {
            NumberOfActiveHandles--;
        }
        void ForceLoad()
        {

        }
    }
}
