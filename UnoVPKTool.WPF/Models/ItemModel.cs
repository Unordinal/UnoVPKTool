using System;
using System.Collections.Generic;
using System.Linq;

namespace UnoVPKTool.WPF.Models
{
    public abstract class ItemModel : IItemModel
    {
        protected int _size;

        public string Path { get; set; }
        public virtual string Name => Path.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
        public virtual string Type => System.IO.Path.GetExtension(Path);
        public virtual int Size => _size;
        public virtual IList<IItemModel> Items { get; } = Array.Empty<IItemModel>();

        public ItemModel(string path, int size)
        {
            Path = path;
            _size = size;
        }
    }
}