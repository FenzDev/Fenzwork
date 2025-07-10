using Fenzwork.Systems.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace Fenzwork.Graphics
{
    public class Sprite : IDataEmbededAsset
    {
        internal Asset<AtlasMetadata> _Metadata;

        public Asset<Texture2D> Texture { get; internal set; }
        public Rectangle SourceRect { get; internal set; }

        internal SpritePresistantData _SpriteData;
        public object PresistantData { get => _SpriteData; set => _SpriteData = (SpritePresistantData)value; }

        public int MaxHorizontalFrames => _SpriteData.AreFramesSizeDetermined ? SourceRect.Width / _SpriteData.FrameWidth : _SpriteData.FramesXNum;
        public int MaxVerticalFrames => _SpriteData.AreFramesSizeDetermined ? SourceRect.Height / _SpriteData.FrameHeight : _SpriteData.FramesYNum;
        
        public int FrameWidth => _SpriteData.AreFramesSizeDetermined? _SpriteData.FrameWidth :  SourceRect.Width / _SpriteData.FramesXNum;
        public int FrameHeight => _SpriteData.AreFramesSizeDetermined ? _SpriteData.FrameHeight : SourceRect.Height / _SpriteData.FramesYNum;

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
                return new Rectangle(SourceRect.X + x * fw, SourceRect.Y + y * fh, fw, fh);
            }
        }

        /// <summary>
        /// Get frame Rectangle used for drawing
        /// </summary>
        /// <param name="x">Frame index by X</param>
        /// <returns></returns>
        public Rectangle this[int x] => this[x, 0];


        public static implicit operator Texture2D(Sprite sprite) => sprite.Texture;

    }

    public class SpritePresistantData
    {
        public bool AreFramesSizeDetermined;
        public int FrameWidth;
        public int FrameHeight;
        public int FramesXNum = 1;
        public int FramesYNum = 1;
    }

    public static class AssetExtensions
    {
        public static Asset<Sprite> WithFrameSize(this Asset<Sprite> sprite, int width, int height)
        {
            SpritePresistantData spriteData = (SpritePresistantData)(sprite.Root.PresistantData ??= new SpritePresistantData());
            spriteData.FrameWidth = width;
            spriteData.FrameHeight = height;
            spriteData.AreFramesSizeDetermined = true;

            return sprite;
        }
        public static Asset<Sprite> WithFramesNumber(this Asset<Sprite> sprite, int numX, int numY)
        {
            SpritePresistantData spriteData = (SpritePresistantData)(sprite.Root.PresistantData ??= new SpritePresistantData());
            spriteData.FramesXNum = numX;
            spriteData.FramesYNum = numY;
            spriteData.AreFramesSizeDetermined = false;

            return sprite;
        }
        public static Asset<Sprite> WithFramesNumber(this Asset<Sprite> sprite, int num) => WithFramesNumber(sprite, num, 1);
    }
}
