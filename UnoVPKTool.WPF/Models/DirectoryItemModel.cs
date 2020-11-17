using System.Collections.Generic;
using System.Linq;

namespace UnoVPKTool.WPF.Models
{
    public record DirectoryItemModel : ItemModel
    {
        public List<ItemModel> Items { get; set; }

        public override int Size => Items.Sum(i => i.Size);

        public DirectoryItemModel(string path, string name, List<ItemModel> items) : base(path, name, 0)
        {
            Items = items;
        }
    }
}