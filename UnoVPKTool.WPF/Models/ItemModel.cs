namespace UnoVPKTool.WPF.Models
{
    public abstract record ItemModel
    {
        protected int _size;

        public string Path { get; set; }
        public string Name { get; set; }
        public virtual int Size => _size;

        public ItemModel(string path, string name, int size)
        {
            (Path, Name) = (path, name);
            _size = size;
        }
    }
}