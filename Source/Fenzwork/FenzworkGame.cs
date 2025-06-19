using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fenzwork.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fenzwork
{
    public class FenzworkGame : Game
    {
        public static string LongName { get; internal set; }
        public static string ShortName { get; internal set; }

        public GraphicsDeviceManager _Graphics;
        public SpriteBatch _SpriteBatch;
        
        public FenzworkGame()
        {
            _Graphics = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            Input.Init();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _SpriteBatch = new SpriteBatch(GraphicsDevice);

        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update(gameTime);
        }

    }
}
