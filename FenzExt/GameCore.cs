using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenzExt
{

    public abstract class GameCore
    {
        internal MGGame _MG;
        public GameWindow Window => _MG.Window;

        protected internal virtual void Init() { }
        protected internal virtual void PreDraw() { }
        protected internal abstract void Draw();
        protected internal virtual void PreUpdate() { }
        protected internal abstract void Update();
    }
}
