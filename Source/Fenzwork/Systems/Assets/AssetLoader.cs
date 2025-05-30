using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.Assets
{
    public abstract class AssetLoader
    {
        public abstract string CategoryName { get; }
        protected ContentManager Content;

        public abstract object LoadRaw(string path);
        public abstract object ReloadRaw(string path, object old);
        public abstract object Load(string name);

    }
}
