using Microsoft.Win32.SafeHandles;

namespace LzhamWrapper.Compression
{
    public class CompressionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public bool IsOpenAndValid => !IsClosed && !IsInvalid;

        public CompressionHandle() : base(true) { }

        public uint? Deinit()
        {
            return IsOpenAndValid ? Lzham.Compression.Deinit(this) : null;
        }

        protected override bool ReleaseHandle()
        {
            Deinit();
            return true;
        }
    }
}