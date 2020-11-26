using Microsoft.Win32.SafeHandles;

namespace LzhamWrapper.Decompression
{
    public class DecompressionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public DecompressionHandle() : base(true) { }

        public uint? Deinit()
        {
            return (!IsClosed && !IsInvalid) ? Lzham.DecompressDeinit(this) : null;
        }

        protected override bool ReleaseHandle()
        {
            Deinit();
            return true;
        }
    }
}