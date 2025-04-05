using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FenzExt.Graphics
{
    public class DrawingHelper
    {
        public GraphicsDevice GDevice { get; internal set; }
        public GraphicsDeviceManager GService {  get; internal set; }
        public SpriteBatch Batch {  get; internal set; }
        public GameTime Time { get; internal set; }

    }
}
