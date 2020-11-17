namespace UnoVPKTool.WPF.Models
{
    public record FileItemModel(string Path, string Name, int Size) : ItemModel(Path, Name, Size)
    {
        public string Extension => System.IO.Path.GetExtension(Path);
    }
}
