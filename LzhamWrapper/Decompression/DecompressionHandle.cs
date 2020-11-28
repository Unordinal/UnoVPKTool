using Microsoft.Win32.SafeHandles;

namespace LzhamWrapper.Decompression
{
    public class DecompressionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public bool IsOpenAndValid => !IsClosed && !IsInvalid;

        public DecompressionHandle() : base(true) { }

        public uint? Deinit()
        {
            return IsOpenAndValid ? Lzham.Decompression.Deinit(this) : null;
        }

        protected override bool ReleaseHandle()
        {
            Deinit();
            return true;
        }
    }
}