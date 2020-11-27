using System;
using System.Runtime.InteropServices;
using LzhamWrapper.Compression;
using LzhamWrapper.Decompression;

#pragma warning disable IDE1006 // Naming Styles

namespace LzhamWrapper
{
    /// <summary>
    /// P/Invoke for LZHAM .dll methods.
    /// </summary>
    internal static class LzhamNative
    {
        private const string LzhamDll = "lzham_x64.dll";

        /// <summary>
        /// Gets the version of the LZHAM .dll.
        /// </summary>
        /// <returns>The version of the LZHAM .dll.</returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint lzham_get_version();

        #region Compression

        /// <summary>
        /// Initializes a compressor.
        /// </summary>
        /// <param name="parameters">Must be initialized via <see cref="CompressionParameters.Initialize"/> before being passed into this method.</param>
        /// <returns>A pointer to the compressor's internal state, or <see langword="null"/> if unsuccessful.</returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern CompressionHandle lzham_compress_init(ref CompressionParameters parameters);

        /// <summary>
        /// Reinitializes a compressor.
        /// </summary>
        /// <param name="state">A valid handle to the compressor that will be reinitialized.</param>
        /// <returns>
        ///     <inheritdoc cref="lzham_compress_init(ref CompressionParameters)"/>
        ///     The returned handle may be located at the same address as the passed-in handle.
        /// </returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern CompressionHandle lzham_compress_reinit(CompressionHandle state);

        /// <summary>
        /// Compresses an arbitrarily-sized block of data, writing as much available compressed data as possible to the output buffer.
        /// This method may be called as many times as needed, but for best perf. try not to call it with tiny buffers.
        /// </summary>
        /// <param name="state">Pointer to internal compression state, created by <see cref="lzham_compress_init(ref CompressionParameters)"/>.</param>
        /// <param name="input">Pointer to an input data buffer.</param>
        /// <param name="inputLength">
        ///     Pointer to a size_t (<see cref="UIntPtr"/>/nuint) containing the number of bytes available in <paramref name="input"/>.
        ///     On return, will be set to the number of bytes read from <paramref name="input"/>.
        /// </param>
        /// <param name="output">Pointer to an output data buffer.</param>
        /// <param name="outputLength">
        ///     Pointer to a size_t (<see cref="UIntPtr"/>/nuint) containing the maximum number of bytes that can be written to <paramref name="output"/>.
        ///     On return, will be set to the number of bytes written to <paramref name="output"/>.
        /// </param>
        /// <param name="noMoreInputBytes">
        ///     Set to <see langword="true"/> to indicate that no more input bytes are available to compress.
        ///     Once you call this function with <paramref name="noMoreInputBytes"/> set to <see langword="true"/>, it must stay set to <see langword="true"/> in all future calls.
        /// </param>
        /// <returns>
        /// <list type="table">
        ///     <para>Normal return status codes:</para>
        ///     <item>
        ///         <term><see cref="CompressStatus.NotFinished"/></term>
        ///         <description>Compression can continue, but the compressor needs more input or more room in the output buffer.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="CompressStatus.NeedsMoreInput"/></term>
        ///         <description>Compression can continue, but the compressor has no more output, has no input, and hasn't been called with <paramref name="noMoreInputBytes"/> set to <see langword="true"/>. Supply more input to continue.</description>
        ///     </item>
        ///     <para>Success/failure return status codes:</para>
        ///     <item>
        ///         <term><see cref="CompressStatus.Success"/></term>
        ///         <description>Compression has completed successfully.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="CompressStatus.Failed"/>, <see cref="CompressStatus.FailedInitializing"/>, <see cref="CompressStatus.InvalidParameter"/></term>
        ///         <description>Something went wrong.</description>
        ///     </item>
        /// </list>
        /// </returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe CompressStatus lzham_compress(CompressionHandle state, byte* input, ref nuint inputLength, byte* output, ref nuint outputLength, byte noMoreInputBytes);

        // XML documentation inheritance (among other XML doc-related things...) is broken when using nint and nuint types. Leaving it here so it'll work when it's fixed.
        /// <inheritdoc cref="lzham_compress(CompressionHandle, in byte[], ref nuint, in byte[], ref nuint, byte)"/>
        /// <param name="flushType"></param>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe CompressStatus lzham_compress2(CompressionHandle state, byte* input, ref nuint inputLength, byte* output, ref nuint outputLength, Flush flushType);

        /// <summary>
        /// Single function call compression interface.
        /// <br/>
        /// Same return codes as <see cref="lzham_compress(CompressionHandle, ref byte[], ref nuint, ref byte[], ref nuint, byte)"/>,
        /// except this function can also return <see cref="CompressStatus.OutputBufferTooSmall"/>.
        /// </summary>
        /// <param name="parameters">The compression parameters to use.</param>
        /// <param name="adler32"></param>
        /// <inheritdoc cref="lzham_compress(CompressionHandle, ref byte[], ref nuint, ref byte[], ref nuint, byte)"/>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe CompressStatus lzham_compress_memory(ref CompressionParameters parameters, byte* output, ref nuint outputLength, byte* input, nuint inputLength, ref uint adler32);

        /// <summary>
        /// Deinitializes a compressor, releasing all allocated memory.
        /// </summary>
        /// <param name="state">A valid handle to the compressor that will be deinitialized.</param>
        /// <returns>adler32 of the source data. Valid only on success.</returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint lzham_compress_deinit(CompressionHandle state);

        #endregion Compression

        #region Decompression

        /// <summary>
        /// Initializes a decompressor.
        /// </summary>
        /// <param name="parameters">Must be initialized via <see cref="DecompressionParameters.Initialize"/> before being passed into this method.</param>
        /// <returns>A pointer to the decompressor's internal state, or <see langword="null"/> if unsuccessful.</returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern DecompressionHandle lzham_decompress_init(ref DecompressionParameters parameters);

        /// <summary>
        /// Reinitializes a compressor.
        /// </summary>
        /// <param name="state">A valid handle to the decompressor that will be reinitialized.</param>
        /// <param name="parameters">Must be initialized via <see cref="DecompressionParameters.Initialize"/> before being passed into this method.</param>
        /// <returns>
        ///     <inheritdoc cref="lzham_decompress_init(ref DecompressionParameters)"/>
        ///     The returned handle may be located at the same address as the passed-in handle.
        /// </returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern DecompressionHandle lzham_decompress_reinit(DecompressionHandle state, ref DecompressionParameters parameters);

        /// <summary>
        /// Decompresses an arbitrarily-sized block of data, writing as much available decompressed data as possible to the output buffer.
        /// This method is implemented as a coroutine so it may be called as many times as needed. However, for best perf. try not to call it with tiny buffers.
        /// </summary>
        /// <param name="state">Pointer to internal decompression state, created by <see cref="lzham_decompress_init(ref DecompressionParameters)"/>.</param>
        /// <param name="input">Pointer to an input data buffer.</param>
        /// <param name="inputLength">
        ///     Pointer to a size_t (<see cref="UIntPtr"/>/nuint) containing the number of bytes available in <paramref name="input"/>.
        ///     On return, will be set to the number of bytes read from <paramref name="input"/>.
        /// </param>
        /// <param name="output">Pointer to an output data buffer.</param>
        /// <param name="outputLength">
        ///     Pointer to a size_t (<see cref="UIntPtr"/>/nuint) containing the maximum number of bytes that can be written to <paramref name="output"/>.
        ///     On return, will be set to the number of bytes written to <paramref name="output"/>.
        /// </param>
        /// <param name="noMoreInputBytes">
        ///     Set to <see langword="true"/> to indicate that no more input bytes are available to decompress.
        ///     Once you call this function with <paramref name="noMoreInputBytes"/> set to <see langword="true"/>, it must stay set to <see langword="true"/> in all future calls.
        /// </param>
        /// <returns>A status code indicating the type of success or failure.</returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe DecompressStatus lzham_decompress(DecompressionHandle state, byte* input, ref nuint inputLength, byte* output, ref nuint outputLength, byte noMoreInputBytes);

        /// <summary>
        /// Single function call decompression interface.
        /// </summary>
        /// <param name="parameters"><inheritdoc cref="lzham_decompress_init(ref DecompressionParameters)"/></param>
        /// <inheritdoc cref="lzham_decompress(DecompressionHandle, byte*, ref nuint, byte*, ref nuint, byte)"/>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe DecompressStatus lzham_decompress_memory(ref DecompressionParameters parameters, byte* output, ref nuint outputLength, byte* input, nuint inputLength, ref uint adler32);

        /// <summary>
        /// Deinitializes a decompressor.
        /// </summary>
        /// <param name="state">A valid handle to the decompressor that will be deinitialized.</param>
        /// <returns>adler32 of decompressed data if <see cref="DecompressionFlags.ComputeAdler32"/> was set; otherwise adler32 of compressed stream.</returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint lzham_decompress_deinit(DecompressionHandle state);

        #endregion Decompression
    }
}

#pragma warning restore IDE1006 // Naming Styles