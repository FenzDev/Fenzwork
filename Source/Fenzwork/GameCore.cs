using Fenzwork.Graphics;
using Fenzwork.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork
{

    public abstract class GameCore
    {
        public MGGame _MG;
        public GameWindow Window => _MG.Window;

        protected internal virtual void Init() { }
        protected internal virtual void PreDraw(DrawingHelper helper) { }
        protected internal abstract void Draw(DrawingHelper helper);
        protected internal virtual void PreUpdate() { }
        protected internal abstract void Update();
        protected internal virtual void PreLoad() { }
        protected internal virtual void Load() { }
        protected internal virtual void Unload() { }
        protected internal virtual void PostUnload() { }
        protected internal virtual bool Error(Exception ex) => true;
    }
}
