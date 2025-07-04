using Fenzwork.Systems.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace Fenzwork.Graphics
{
    public class Sprite
    {
        internal Asset<AtlasMetadata> _Metadata;

        public Asset<Texture2D> Texture { get; internal set; }
        public Rectangle SourceRect { get; internal set; }

        private SpriteStaticData _FrameData;
        public SpriteStaticData FrameData { get => _FrameData; internal set => _FrameData = value; }

        int FrameWidth => FrameData.AreFramesSizeDetermined? FrameData.FrameWidth : FrameData.FramesXNum / SourceRect.Width;
        int FrameHeight => FrameData.AreFramesSizeDetermined ? FrameData.FrameHeight : FrameData.FramesYNum / SourceRect.Height;

        public void SetFrameSize(int w, int h)
        {
            _FrameData.FrameWidth = w; _FrameData.FrameHeight = h;
            _FrameData.AreFramesSizeDetermined = true;
        }

        public void SetFramesNumber(int x, int y)
        {
            _FrameData.FramesXNum = x; _FrameData.FramesYNum = y;
            _FrameData.AreFramesSizeDetermined = false;
        }

        /// <summary>
        /// Get frame Rectangle used for drawing
        /// </summary>
        /// <param name="x">Frame index by X</param>
        /// <param name="y">Frame index by Y</param>
        /// <returns></returns>
        public Rectangle this[int x, int y]
        {
            get
            {
                int fw = FrameWidth;
                int fh = FrameHeight;
                return new Rectangle(x * fw, y * fh, fw, fh);
            }
        }
    }

    public record struct SpriteStaticData(
        bool AreFramesSizeDetermined,
        int FrameWidth,
        int FrameHeight,
        int FramesXNum,
        int FramesYNum
    );
}
