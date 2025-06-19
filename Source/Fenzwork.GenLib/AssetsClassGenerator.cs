using Fenzwork.GenLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.GenLib
{
    public class AssetsClassGenerator
    {
        public static StreamWriter Writer;

        public static AssetsClassNode ClassNodes = new() { Name = "Assets", Members = [ ] };

        public static void WriteClass()
        {
            Writer.WriteLine("using Fenzwork;");
            Writer.WriteLine("using Fenzwork.Systems.Assets;");
            Writer.WriteLine($"namespace {GenManager.Namespace}\n{{");
            WriteNodes(ClassNodes, "\t");
            Writer.WriteLine("}");
        }
        public static void WriteNodes(AssetsClassNode node, string indent)
        {
            Writer.Write($"{indent}public static ");
            var isProperty = node.Members == null;
            if (isProperty)
            {
                Writer.WriteLine($"Asset<{node.PropertyType}> {node.Name} => AssetsManager.Get<{node.PropertyType}>(\"{node.AssetName.Replace("\\", "\\\\")}\");");
                return;
            }

            Writer.WriteLine($"class {node.Name}");
            Writer.WriteLine($"{indent}{{");
            foreach (var subNode in node.Members!)
                WriteNodes(subNode.Value, $"{indent}\t");
            Writer.WriteLine($"{indent}}}");

        }

        public static void Include(AssetsGroupConfig config, string assetName)
        {
            var assetNameSplit = assetName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var currentNode = ClassNodes;
            for (int i = 0; i < assetNameSplit.Length; i++)
            {
                if (i == assetNameSplit.Length - 1)
                {
                    var newPropertyNode = new AssetsClassNode() { Name = assetNameSplit[i].Split('.')[0].Replace(" ", "").Replace('-', '_'), AssetName = assetName, PropertyType = config.LoadAs };
                    currentNode.Members!.Add(newPropertyNode.Name, newPropertyNode);
                    break;
                }

                AddParentNodes(ref currentNode, assetNameSplit[i].Split('.'));
            }
        }

        /// This is for when including a directory name seperated by '.'.
        static void AddParentNodes(ref AssetsClassNode currentNode, string[] names)
        {
            foreach (var name in names)
            {
                if (currentNode.Members!.TryGetValue(name, out var node))
                    currentNode = node;
                else
                {
                    var newParentNode = new AssetsClassNode() { Name = name, Members = [] };
                    currentNode.Members.Add(name, newParentNode);
                    currentNode = newParentNode;
                }

            }
        }
    }
}
