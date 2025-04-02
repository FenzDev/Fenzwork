using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FenzExt.InputSystem;
using System;
using FenzExt.Graphics;

namespace FenzExt
{
    internal class MGGame : Game
    {
        internal GraphicsDeviceManager _Graphics;
        internal SpriteBatch _SpriteBatch;
        internal GameCore _Core;
        internal bool _WillQuit;
        internal DrawingHelper drawingHelper = new DrawingHelper();

        public MGGame(GameCore core)
        {
            _Graphics = new GraphicsDeviceManager(this);
            _Core = core;
            _Core._MG = this;
            Content.RootDirectory = "Assets";

        }
        
        protected override void Initialize()
        {
            Input.Init();
            _Core.Init();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _SpriteBatch = new SpriteBatch(GraphicsDevice);
        
            drawingHelper.Batch = _SpriteBatch;
            drawingHelper.GService = _Graphics;
            drawingHelper.GDevice = GraphicsDevice;
        }

        protected override void Update(GameTime gameTime)
        {
            _Core.PreUpdate();
            Input.Update(gameTime);
            _Core.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            _Core.PreDraw(drawingHelper);
            _Core.Draw(drawingHelper);
        }
    }
}
