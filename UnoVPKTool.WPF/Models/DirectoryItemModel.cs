using System.Collections.Generic;
using System.Linq;

namespace UnoVPKTool.WPF.Models
{
    public class DirectoryItemModel : ItemModel
    {
        public override IList<IItemModel> Items { get; }

        public override int Size => Items.Sum(i => i.Size);

        public DirectoryItemModel(string path, IList<IItemModel>? items = null) : base(path, 0)
        {
            Items = items ?? new List<IItemModel>();
        }
    }
}