namespace UnoVPKTool.WPF.Models
{
    public class BlockFileItemModel : FileItemModel
    {
        public string ArchivePath { get; set; }

        public int CompressedSize { get; set; }

        public bool IsCompressed => Size != CompressedSize;

        public BlockFileItemModel(string path, int size, string archivePath, int compressedSize) : base(path, size)
        {
            ArchivePath = archivePath;
            CompressedSize = compressedSize;
        }

        public BlockFileItemModel(string path, int size, string archivePath) : this(path, size, archivePath, -1) { }
    }
}