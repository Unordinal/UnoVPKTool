namespace UnoVPKTool.WPF.Models
{
    public abstract class ItemModel
    {
        protected int _size;

        public string Path { get; set; }
        public string Name { get; set; }
        public virtual string Type => System.IO.Path.GetExtension(Path);
        public virtual int Size => _size;

        public ItemModel(string path, int size)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            _size = size;
        }
    }
}