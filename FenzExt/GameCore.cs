using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenzExt
{

    public abstract class GameCore
    {
        protected internal virtual void Init() { }
        protected internal virtual void PreDraw() { }
        protected internal abstract void Draw();
        protected internal virtual void PreUpdate() { }
        protected internal abstract void Update();
    }
}
