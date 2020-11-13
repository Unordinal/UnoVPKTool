using System.Diagnostics;
using System.IO;
using UnoVPKTool.Extensions;

namespace UnoVPKTool.VPK
{
    // Instance Methods
    public class Tree
    {
        /// <summary>
        /// The root node of the tree. Contains file extensions.
        /// </summary>
        public ITreeNode RootNode { get; set; }

        public Tree(BinaryReader reader)
        {
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
                        var entryBlock = new EntryBlock(reader) { FilePath = fullFilePath };
                        ITreeNode fileNode = new TreeNode(filename, entryBlock, pathNode);

                        //Debug.WriteLine(filename);
                    }
                }
            }
        }
    }
}