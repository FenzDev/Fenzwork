using Fenzwork.Systems.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Fenzwork.Systems.GUI
{
    public struct GuiGPUStates
    {
        public GuiGPUStates()
        {
             
        }

        public GuiGPUStates(Asset<Texture2D> texture, Asset<Effect> effect, Rectangle? clipMask)
        {
            Texture = texture;
            Effect = effect;
            ClipMask = clipMask;
            CreateHashCode();
        }

        public bool IsActive = true;
        public Asset<Texture2D> Texture;
        public Asset<Effect> Effect;
        public Rectangle? ClipMask;

        private int? _Hash;
        private int CreateHashCode() => (_Hash = HashCode.Combine(Texture.Content, Effect.Content, ClipMask)).Value;
        public override int GetHashCode() => _Hash ?? CreateHashCode();
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is GuiGPUStates other)
            {
                return  GetHashCode() == other.GetHashCode()
                        && Texture.Content == other.Texture.Content
                        && Effect.Content == other.Effect.Content
                        && ClipMask == other.ClipMask;
            }
            return false;
        }
    }
}
