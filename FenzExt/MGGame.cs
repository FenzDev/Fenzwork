using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FenzExt.InputSystem;
using System;
using FenzExt.Graphics;
using FenzExt.AssetsSystem;

namespace FenzExt
{
    public class MGGame : Game
    {
        public static MGGame _Instance;
        public GraphicsDeviceManager _Graphics;
        public SpriteBatch _SpriteBatch;
        public GameCore _Core;
        public DrawingHelper drawingHelper = new DrawingHelper();

        public MGGame(GameCore core)
        {
            _Graphics = new GraphicsDeviceManager(this);
            _Core = core;
            _Core._MG = this;
            Content.RootDirectory = "Assets";

        }
        
        protected override void Initialize()
        {
#if !DEBUG
            try
            {
#endif
            Input.Init();
            _Core.Init();
#if !DEBUG
            }
            catch (Exception ex)
            {
                mg._Core.Error(ex);
            }
#endif
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _SpriteBatch = new SpriteBatch(GraphicsDevice);
        
            drawingHelper.Batch = _SpriteBatch;
            drawingHelper.GService = _Graphics;
            drawingHelper.GDevice = GraphicsDevice;
#if !DEBUG
            try
            {
#endif
                _Core.PreLoad(); 
                Assets.Load(this);
                _Core.Load();
#if !DEBUG
            }
            catch (Exception ex)
            {
                mg._Core.Error(ex);
            }
#endif
        }

        protected override void UnloadContent()
        {
#if !DEBUG
            try
            {
#endif
            _Core.Unload();
            Assets.Unload(this);
            _Core.PostUnload();
#if !DEBUG
            }
            catch (Exception ex)
            {
                mg._Core.Error(ex);
            }
#endif
        }

        protected override void Update(GameTime gameTime)
        {
#if !DEBUG
            try
            {
#endif
            _Core.PreUpdate();
            Input.Update(gameTime);
            _Core.Update();
#if !DEBUG
            }
            catch (Exception ex)
            {
                mg._Core.Error(ex);
            }
#endif
        }

        protected override void Draw(GameTime gameTime)
        {
#if !DEBUG
            try
            {
#endif
                drawingHelper.Time = gameTime;
                _Core.PreDraw(drawingHelper);
                _Core.Draw(drawingHelper);
#if !DEBUG
            }
            catch (Exception ex)
            {
                mg._Core.Error(ex);
            }
#endif
        }
    }
}
