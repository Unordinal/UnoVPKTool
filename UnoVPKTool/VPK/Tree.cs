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

            string extension;
            while (!string.IsNullOrEmpty(extension = reader.ReadNullTermString()))
            {
                ITreeNode extNode = new TreeNode(extension, null, RootNode);

                string path;
                while (!string.IsNullOrEmpty(path = reader.ReadNullTermString()))
                {
                    ITreeNode pathNode = new TreeNode(path, null, extNode);

                    string filename;
                    while (!string.IsNullOrEmpty(filename = reader.ReadNullTermString()))
                    {
                        string fullFilePath = Path.Combine(path.Trim(), filename + "." + extension).Replace('/', Path.DirectorySeparatorChar);
                        var entryBlock = new DirectoryEntryBlock(reader) { FilePath = fullFilePath };
                        ITreeNode fileNode = new TreeNode(filename, entryBlock, pathNode);

                        entryBlocks.Add(entryBlock);
                        //Debug.WriteLine(filename);
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