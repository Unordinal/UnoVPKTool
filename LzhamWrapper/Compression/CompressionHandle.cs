using Microsoft.Win32.SafeHandles;

namespace LzhamWrapper.Compression
{
    public class CompressionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public CompressionHandle() : base(true) { }

        public uint? Deinit()
        {
            return (!IsClosed && !IsInvalid) ? Lzham.CompressDeinit(this) : null;
        }

        protected override bool ReleaseHandle()
        {
            Deinit();
            return true;
        }
    }
}