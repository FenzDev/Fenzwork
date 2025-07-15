using Fenzwork.Systems.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Fenzwork.Systems.GUI
{
    public class GuiLoader(GuiAssetType type) : RawAssetLoader
    {
        public GuiAssetType Type = type;

        protected override void Load(Stream stream, AssetID assetID, out object resultAsset)
        {
            resultAsset = null;
            switch (type)
            {
                case GuiAssetType.View:
                    resultAsset = LoadView(stream, assetID);
                    break;
                case GuiAssetType.Widget:
                    // TODO : Widget loader
                    break;
                case GuiAssetType.Style:
                    // TODO: Style loader
                    break;
            }
        }

        private GuiView LoadView(Stream stream, AssetID assetID)
        {
            var view = new GuiView();
            view.View = view;

            var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true,  });

            try
            {
                reader.Read();
                if (reader.Name != "View")
                    throw new Exception("Wrong format <View> root element was expected.");
                
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element) 
                        ReadElement(view, reader);
                }
            }
            catch (Exception ex)
            {
                var info = (IXmlLineInfo)reader;

                throw new GuiLoaderException(assetID, info.LineNumber, info.LinePosition, ex.Message, ex);
            }

            return view;
        }

        private void ReadElement(GuiParent guiParent, XmlReader reader )
        {
            if (GuiManager._AvailableElements.TryGetValue(reader.Name, out var type))
            {
                var obj = (GuiComponent)Activator.CreateInstance(type);
                obj.View = guiParent.View;
                
                obj.Read(reader);
                
                guiParent.Children.Add(obj);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

    }

    public enum GuiAssetType
    {
        View,
        Widget,
        Style,
    }
}
