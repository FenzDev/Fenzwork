using System.Collections.Immutable;
using System.IO;
using Fenzwork.Systems.Assets;

namespace Fenzwork.Systems.Assets.Loaders
{
    public class TextLoader : AssetLoader<string>
    {
        public override string CategoryName { get; protected set; } = "Texts";

        public override ImmutableArray<string> FileExtensions { get; } = ["txt"];

        public override string DefaultObject { get; protected set; } = "";

        public override string Load(string name, Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public override string Reload(string old, string name, Stream stream)
        {
            return Load(name, stream);
        }
    }
}
