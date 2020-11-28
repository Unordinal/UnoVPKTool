namespace LzhamWrapper
{
    // TODO: Add overloads for compress/decompress that take Span<byte> or byte[], both with offset + length (for 'ref' purposes - maybe use 'out'?)
    public static partial class Lzham
    {
        /// <inheritdoc cref="LzhamNative.GetVersion"/>
        ///
        public static uint GetVersion()
        {
            return LzhamNative.GetVersion();
        }
    }
}