using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenzExt.AssetsSystem
{
    public interface IAssetLoader
    {
        public ImmutableArray<string> FileExtensions { get; }

        public string CategoryName { get; }

        public object DefaultObject { get; }

        object Load(string name, Stream stream);
        object Reload(object old, string name, Stream stream);
    }
}
