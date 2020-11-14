using System;
using System.Collections.Generic;

namespace UnoVPKTool.VPK
{
    public interface ITreeNode
    {
        public const string NodeEnd = "";

        /// <summary>
        /// The header of this <see cref="ITreeNode"/>. This may be a file extension, file path, or file name.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// The <see cref="VPK.DirectoryEntryBlock"/> this node contains.
        /// </summary>
        public DirectoryEntryBlock? EntryBlock { get; set; }

        /// <summary>
        /// The parent of this node.
        /// </summary>
        public ITreeNode? Parent { get; set; }

        /// <summary>
        /// The children of this node.
        /// </summary>
        public IDictionary<string, ITreeNode> Children { get; }

        public ITreeNode this[string key] { get; set; }

        /// <summary>
        /// Returns true if the specified node is a child of this node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool ContainsChild(ITreeNode node);

        /// <summary>
        /// Adds a child to this node and returns it.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ITreeNode AddChild(ITreeNode node);

        /// <summary>
        /// Removes a child from this node and returns it.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ITreeNode RemoveChild(ITreeNode node);

        /// <summary>
        /// Traverses the node tree and executes the specified function on each node.
        /// </summary>
        /// <param name="visitor"></param>
        public void Traverse(Action<ITreeNode> visitor);

        /// <summary>
        /// Pretty-prints the tree from this node onwards.
        /// </summary>
        /// <param name="indent"></param>
        /// <param name="last"></param>
        /// <returns></returns>
        public string PrintTree(string indent = "", bool last = true);
    }
}