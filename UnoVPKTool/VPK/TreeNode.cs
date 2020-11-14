using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnoVPKTool.VPK
{
    /// <summary>
    /// A node in a <see cref="VPK.Tree"/>.
    /// </summary>
    public class TreeNode : ITreeNode
    {
        private ITreeNode? _parent;

        public string Header { get; set; }

        public DirectoryEntryBlock? EntryBlock { get; set; }

        public ITreeNode? Parent
        {
            get => _parent;
            set
            {
                if (_parent == value) return;

                _parent?.RemoveChild(this);
                _parent = value;
            }
        }

        public IDictionary<string, ITreeNode> Children { get; }

        public ITreeNode this[string key]
        {
            get => Children[key];
            set
            {
                if (Children.TryGetValue(key, out var oldNode)) oldNode.Parent = null;
                Children[key] = value;
            }
        }

        public TreeNode(string header, DirectoryEntryBlock? entryBlock = null, ITreeNode? parent = null, IDictionary<string, ITreeNode>? children = null)
        {
            Header = header;
            EntryBlock = entryBlock;
            Parent = parent;
            Children = children ?? new Dictionary<string, ITreeNode>();

            if (Parent is not null) Parent.AddChild(this);
            foreach (var child in Children.Values)
            {
                if (child.ContainsChild(this)) child.RemoveChild(this);
                child.Parent = this;
            }
        }

        public bool ContainsChild(ITreeNode node)
        {
            return Children.ContainsKey(node.Header);
        }

        public ITreeNode AddChild(ITreeNode node)
        {
            Children.Add(node.Header, node);
            node.Parent = this;
            return node;
        }

        public ITreeNode RemoveChild(ITreeNode node)
        {
            if (ContainsChild(node)) Children.Remove(node.Header);
            if (node.Parent == this) node.Parent = null;
            return node;
        }

        public void Traverse(Action<ITreeNode> visitor)
        {
            visitor(this);
            foreach (var child in Children.Values) child.Traverse(visitor);
        }

        public string PrintTree(string indent = "", bool last = true)
        {
            StringBuilder sb = new StringBuilder();
            if (Parent is null)
                sb.AppendLine(Header);
            else
                sb.AppendLine(indent + (last ? "└╴" : "├╴") + Header);

            indent += last ? "  " : "│ ";

            IList<ITreeNode> childList = Children.Values.OrderBy((n) => n.Header).ToList();
            for (int i = 0; i < childList.Count; i++)
            {
                sb.Append(childList[i].PrintTree(indent, i == (childList.Count - 1)));
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            string output =
                $"[{Header}]: (Parent: {Parent?.Header ?? "<none>"}, Children: {Children.Count})";

            if (EntryBlock is not null) output += "\nEntry:\n" + Utils.IndentString(EntryBlock.ToString()!);

            return output;
        }
    }
}