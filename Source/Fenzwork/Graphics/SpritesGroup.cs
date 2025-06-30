using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Graphics
{
    public class SpritesAssembly
    {
        public Texture2D[] Atlases { get; internal set; }
        public Dictionary<string, Sprite> Sprites { get; internal set; }
    }
}
