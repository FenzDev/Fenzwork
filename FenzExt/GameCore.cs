using FenzExt.Graphics;
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
        protected internal virtual void PreDraw(DrawingHelper helper) { }
        protected internal abstract void Draw(DrawingHelper helper);
        protected internal virtual void PreUpdate() { }
        protected internal abstract void Update();
        protected internal virtual void Error(Exception ex) { }
    }
}
