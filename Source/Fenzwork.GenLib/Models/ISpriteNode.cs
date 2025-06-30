using RectpackSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.GenLib.Models
{
    public interface ISpriteNode
    {
        public bool IsFolder { get; }
        public int TotalArea { get; }
        public string Name { get; set; }
    }

    public class FolderSpriteNode : ISpriteNode
    {
        public string Name { get; set; }
        public bool IsOrdererd { get; set; }
        public bool IsFolder => true;
        public int TotalFilesNumber { get; set; }
        public int TotalArea { get; set; }
        public uint TotalAreaWithPadding { get; set; }
        public List<FolderSpriteNode> SubFolders = [];
        public List<FileSpriteNode> Files = [];
    }

    public class FileSpriteNode : ISpriteNode
    {
        public string Name { get; set; }
        public bool IsFolder => false;
        public string FullName { get; set; }
        public Size Size { get; set; }
        public int TotalArea => Size.Width * Size.Height;

    }

}
