using System;
using Microsoft.Win32.SafeHandles;

namespace LzhamWrapper.Decompression
{
    public class DecompressionHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public DecompressionHandle() : base(true) { }

        public uint Deinit()
        {
            return Lzham.DecompressDeinit(this);
        }

        protected override bool ReleaseHandle()
        {
            Deinit();
            return true;
        }
    }
}