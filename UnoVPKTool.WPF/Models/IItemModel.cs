using System.Collections;
using System.Collections.Generic;

namespace UnoVPKTool.WPF.Models
{
    public interface IItemModel
    {
        public string Path { get; }

        public string Name { get; }

        public string Type { get; }

        public int Size { get; }

        public IList<IItemModel> Items { get; }
    }
}