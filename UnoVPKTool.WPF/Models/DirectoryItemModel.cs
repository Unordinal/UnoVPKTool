using System.Collections.Generic;
using System.Linq;

namespace UnoVPKTool.WPF.Models
{
    public class DirectoryItemModel : ItemModel
    {
        public IList<ItemModel> Items { get; }

        public override int Size => Items.Sum(i => i.Size);

        public DirectoryItemModel(string path, IList<ItemModel>? items) : base(path, 0)
        {
            Items = items ?? new List<ItemModel>();
        }
    }
}