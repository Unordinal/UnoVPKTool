using System.Collections.Generic;
using System.IO;
using UnoVPKTool.Extensions;

namespace UnoVPKTool.VPK
{
    // Instance Methods
    public class Tree : IBinaryWritable
    {
        /// <summary>
        /// The root node of the tree. Contains file extensions.
        /// </summary>
        public ITreeNode RootNode { get; set; }

        public Tree(BinaryReader reader, out IList<DirectoryEntryBlock> entryBlocks)
        {
            entryBlocks = new List<DirectoryEntryBlock>();
            RootNode = new TreeNode("root");

            string extension, path, name;
            while (!string.IsNullOrEmpty(extension = reader.ReadNullTermString()))
            {
                ITreeNode extNode = new TreeNode(extension, null, RootNode);

                while (!string.IsNullOrEmpty(path = reader.ReadNullTermString()))
                {
                    ITreeNode pathNode = new TreeNode(path, null, extNode);

                    while (!string.IsNullOrEmpty(name = reader.ReadNullTermString()))
                    {
                        string fullFilePath = Utils.GetFilePathFromVPKParts(extension, path, name);
                        var entryBlock = new DirectoryEntryBlock(reader, fullFilePath);
                        ITreeNode fileNode = new TreeNode(name, entryBlock, pathNode);

                        entryBlocks.Add(entryBlock);
                    }
                }
            }
        }

        public Tree(BinaryReader reader) : this(reader, out _) { }

        public void Write(BinaryWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}