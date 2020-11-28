using System;
using System.Runtime.InteropServices;
using LzhamWrapper.Compression;

namespace LzhamWrapper
{
    internal static partial class LzhamNative
    {
        internal static class Compression
        {
            /// <summary>
            /// Initializes a compressor.
            /// </summary>
            /// <param name="parameters">Must be initialized via <see cref="CompressionParameters.Initialize"/> before being passed into this method.</param>
            /// <returns>A pointer to the compressor's internal state, or <see langword="null"/> if unsuccessful.</returns>
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_compress_init")]
            internal static extern CompressionHandle Init(ref CompressionParameters parameters);

            /// <summary>
            /// Reinitializes a compressor.
            /// </summary>
            /// <param name="state">A valid handle to the compressor that will be reinitialized.</param>
            /// <returns>
            ///     <inheritdoc cref="Init(ref CompressionParameters)"/>
            ///     The returned handle may be located at the same address as the passed-in handle.
            /// </returns>
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_compress_reinit")]
            internal static extern CompressionHandle Reinit(CompressionHandle state);

            /// <summary>
            /// Compresses an arbitrarily-sized block of data, writing as much available compressed data as possible to the output buffer.
            /// This method may be called as many times as needed, but for best perf. try not to call it with tiny buffers.
            /// </summary>
            /// <param name="state">Pointer to internal compression state, created by <see cref="Init(ref CompressionParameters)"/>.</param>
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
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_compress")]
            internal static extern unsafe CompressStatus Compress(CompressionHandle state, byte* input, ref nuint inputLength, byte* output, ref nuint outputLength, byte noMoreInputBytes);

            // XML documentation inheritance (among other XML doc-related things...) is broken when using nint and nuint types. Leaving it here so it'll work when it's fixed.
            /// <param name="flushType">Flush type.</param>
            /// <inheritdoc cref="Compress(CompressionHandle, byte*, ref nuint, byte*, ref nuint, byte)"/>
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_compress2")]
            internal static extern unsafe CompressStatus Compress(CompressionHandle state, byte* input, ref nuint inputLength, byte* output, ref nuint outputLength, Flush flushType);

            /// <summary>
            /// Single function call compression interface.
            /// <br/>
            /// Same return codes as <see cref="Compress(CompressionHandle, byte*, ref nuint, byte*, ref nuint, byte)"/>,
            /// except this function can also return <see cref="CompressStatus.OutputBufferTooSmall"/>.
            /// </summary>
            /// <param name="parameters">The compression parameters to use.</param>
            /// <param name="adler32">The adler32 of the source data.</param>
            /// <inheritdoc cref="Compress(CompressionHandle, byte*, ref nuint, byte*, ref nuint, byte)"/>
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_compress_memory")]
            internal static extern unsafe CompressStatus CompressMemory(ref CompressionParameters parameters, byte* output, ref nuint outputLength, byte* input, nuint inputLength, ref uint adler32);

            /// <summary>
            /// Deinitializes a compressor, releasing all allocated memory.
            /// </summary>
            /// <param name="state">A valid handle to the compressor that will be deinitialized.</param>
            /// <returns>adler32 of the source data. Valid only on success.</returns>
            [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_compress_deinit")]
            internal static extern uint Deinit(CompressionHandle state);
        }
    }
}