using Fenzwork.Systems.Assets;
using Fenzwork.Systems.GUI.Components;
using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Fenzwork.Systems.GUI
{
    public static class GuiManager
    {
        // TODO: support multiple views
        public static Asset<GuiView> CurrentView { get; private set; }
        public static GuiRenderer Renderer { get; private set; }

        private static SpriteBatch _SBatch;

        internal static Dictionary<string, Type> _AvailableElements = [];

        public static void SetView(Asset<GuiView> view)
        {
            view.Content.Renderer = Renderer;
            CurrentView = view;
            CleanChildrenGraphics(view);
        }

        private static void CleanChildrenGraphics(GuiParent parent)
        {
            for (int c = 0; c < parent.Children.Count; c++)
                if (parent.Children[c] is GuiParent subParent)
                    CleanChildrenGraphics(subParent);
                else if (parent.Children[c] is GuiVisualComponent visual)
                    visual.MarkDirty();

        }

        public static void Init(GraphicsDevice gd)
        {
            _AvailableElements.Add("Rectangle", typeof(RectangleComponent));

            Renderer = new GuiRenderer(gd);

            _SBatch = new SpriteBatch(gd);
        }

        public static void Render() => Renderer.Render();

    }
}
