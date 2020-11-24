using UnoVPKTool.VPK;

namespace UnoVPKTool.WPF.Models
{
    public class BlockFileItemModel : FileItemModel
    {
        public DirectoryEntryBlock Block { get; set; }

        public BlockFileItemModel(string path, DirectoryEntryBlock block) : base(path, (int)block.TotalUncompressedSize)
        {
            Block = block;
        }
    }
}