using Microsoft.Win32.SafeHandles;

namespace LzhamWrapper.Compression
{
    public class CompressionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public CompressionHandle() : base(true) { }

        public uint Deinit()
        {
            return Lzham.CompressDeinit(this);
        }

        protected override bool ReleaseHandle()
        {
            Deinit();
            return true;
        }
    }
}