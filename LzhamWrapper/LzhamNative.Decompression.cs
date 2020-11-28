using System;
using System.Runtime.InteropServices;
using LzhamWrapper.Decompression;

namespace LzhamWrapper
{
    internal static partial class LzhamNative
    {
        internal static class Decompression
        {
            /// <summary>
            /// Initializes a decompressor.
            /// </summary>
            /// <param name="parameters">Must be initialized via <see cref="DecompressionParameters.Initialize"/> before being passed into this method.</param>
            /// <returns>A pointer to the decompressor's internal state, or <see langword="null"/> if unsuccessful.</returns>
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_decompress_init")]
            internal static extern DecompressionHandle Init(ref DecompressionParameters parameters);

            /// <summary>
            /// Reinitializes a compressor.
            /// </summary>
            /// <param name="state">A valid handle to the decompressor that will be reinitialized.</param>
            /// <param name="parameters">Must be initialized via <see cref="DecompressionParameters.Initialize"/> before being passed into this method.</param>
            /// <returns>
            ///     <inheritdoc cref="Init(ref DecompressionParameters)"/>
            ///     The returned handle may be located at the same address as the passed-in handle.
            /// </returns>
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_decompress_reinit")]
            internal static extern DecompressionHandle Reinit(DecompressionHandle state, ref DecompressionParameters parameters);

            /// <summary>
            /// Decompresses an arbitrarily-sized block of data, writing as much available decompressed data as possible to the output buffer.
            /// This method is implemented as a coroutine so it may be called as many times as needed. However, for best perf. try not to call it with tiny buffers.
            /// </summary>
            /// <param name="state">Pointer to internal decompression state, created by <see cref="Init(ref DecompressionParameters)"/>.</param>
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
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_decompress")]
            internal static extern unsafe DecompressStatus Decompress(DecompressionHandle state, byte* input, ref nuint inputLength, byte* output, ref nuint outputLength, byte noMoreInputBytes);

            /// <summary>
            /// Single function call decompression interface.
            /// </summary>
            /// <param name="parameters"><inheritdoc cref="Init(ref DecompressionParameters)"/></param>
            /// <param name="adler32">adler32 of the source data.</param>
            /// <inheritdoc cref="lzham_decompress(DecompressionHandle, byte*, ref nuint, byte*, ref nuint, byte)"/>
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_decompress_memory")]
            internal static extern unsafe DecompressStatus DecompressMemory(ref DecompressionParameters parameters, byte* output, ref nuint outputLength, byte* input, nuint inputLength, ref uint adler32);

            /// <summary>
            /// Deinitializes a decompressor.
            /// </summary>
            /// <param name="state">A valid handle to the decompressor that will be deinitialized.</param>
            /// <returns>adler32 of decompressed data if <see cref="DecompressionFlags.ComputeAdler32"/> was set; otherwise adler32 of compressed stream.</returns>
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_decompress_deinit")]
            internal static extern uint Deinit(DecompressionHandle state);
        }
    }
}