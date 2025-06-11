using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Fenzwork.Graphics;
using System.Reflection;

namespace Fenzwork.Services
{
    public class MGGame : Game
    {
        public static MGGame Instance;
        public GraphicsDeviceManager _Graphics;
        public SpriteBatch _SpriteBatch;
        public GameCore _Core;
        public DrawingHelper drawingHelper = new DrawingHelper();

        public MGGame(GameCore core)
        {
            _Graphics = new GraphicsDeviceManager(this);
            _Core = core;
            _Core._MG = this;
            Instance = this;
        }

        protected override void Initialize()
        {
            Input.Init();
            AssetsManager.Init();
            DebugMessenger.Init();
            _Core.Init();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _SpriteBatch = new SpriteBatch(GraphicsDevice);

            drawingHelper.Batch = _SpriteBatch;
            drawingHelper.GService = _Graphics;
            drawingHelper.GDevice = GraphicsDevice;
            _Core.PreLoad();
            //Assets.Load(this);
            _Core.Load();
        }

        protected override void UnloadContent()
        {
            _Core.Unload();
            //Assets.Unload(this);
            _Core.PostUnload();

            DebugMessenger.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            _Core.PreUpdate();
            DebugMessenger.Tick();
            Input.Update(gameTime);
            _Core.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            drawingHelper.Time = gameTime;
            _Core.PreDraw(drawingHelper);
            _Core.Draw(drawingHelper);
        }
    }
}
