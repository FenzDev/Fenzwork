using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public abstract class AssetLoader<T> : IAssetLoader
    {
        public abstract ImmutableArray<string> FileExtensions { get; }

        public abstract string CategoryName { get; protected set; }
        public abstract T DefaultObject { get; protected set; }

        string IAssetLoader.CategoryName => CategoryName;
        object IAssetLoader.DefaultObject => DefaultObject;

        public abstract T Load(string name, Stream stream);
        public abstract T Reload(T old, string name, Stream stream);

        public object Reload(object old, string name, Stream stream) => Reload(old, name, stream);

        object IAssetLoader.Load(string name, Stream stream) => Load(name, stream);
    }
}
