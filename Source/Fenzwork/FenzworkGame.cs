using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fenzwork.Graphics;
using Fenzwork.Systems.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fenzwork
{
    public abstract class FenzworkGame : Game
    {
        internal static Game _GameSingleton { get; private set; }

        public static string LongName { get; internal set; }
        public static string ShortName { get; internal set; }

        public GraphicsDeviceManager _Graphics;
        public SpriteBatch _SpriteBatch;

        public FenzworkGame(string longName, string shortName)
        {
            LongName = longName;
            ShortName = shortName;
            _Graphics = new GraphicsDeviceManager(this);
            _GameSingleton = this;
        }

        protected override void Initialize()
        {
            Input.Init();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _SpriteBatch = new SpriteBatch(GraphicsDevice);
            AssetsManager.InternalInit(GetType().Assembly);
        }

        protected override void Update(GameTime gameTime)
        {
            AssetsManager.Tick();
            Input.Update(gameTime);
        }

    }
}
