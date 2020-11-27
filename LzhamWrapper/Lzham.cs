using System;
using LzhamWrapper.Compression;
using LzhamWrapper.Decompression;

namespace LzhamWrapper
{
    // TODO: Add overloads for compress/decompress that take Span<byte> or byte[], both with offset + length (for 'ref' purposes - maybe use 'out'?)
    public static class Lzham
    {
        /// <inheritdoc cref="LzhamNative.lzham_get_version"/>
        /// 
        public static uint GetVersion()
        {
            return LzhamNative.lzham_get_version();
        }

        #region Compression

        /// <param name="parameters">The compression parameters to use for the compressor.</param>
        /// <inheritdoc cref="LzhamNative.lzham_compress_init(ref CompressionParameters)"/>
        public static CompressionHandle CompressInit(CompressionParameters parameters)
        {
            parameters.Initialize();
            return LzhamNative.lzham_compress_init(ref parameters);
        }

        /// <inheritdoc cref="LzhamNative.lzham_compress_reinit(CompressionHandle)"/>
        ///
        public static CompressionHandle CompressReinit(CompressionHandle state)
        {
            return LzhamNative.lzham_compress_reinit(state);
        }

        /// <inheritdoc cref="LzhamNative.lzham_compress(CompressionHandle, byte*, ref nuint, byte*, ref nuint, byte)"/>
        ///
        public static unsafe CompressStatus Compress(CompressionHandle state, ReadOnlySpan<byte> input, Span<byte> output, bool noMoreInputBytes)
        {
            nuint inputLength = (uint)input.Length;
            nuint outputLength = (uint)output.Length;
            fixed (byte* pInput = input, pOutput = output)
            {
                return LzhamNative.lzham_compress(state, pInput, ref inputLength, pOutput, ref outputLength, noMoreInputBytes ? 1 : 0);
            }
        }

        /// <inheritdoc cref="LzhamNative.lzham_compress2(CompressionHandle, byte*, ref nuint, byte*, ref nuint, Flush)"/>
        ///
        public static unsafe CompressStatus Compress(CompressionHandle state, ReadOnlySpan<byte> input, Span<byte> output, Flush flushType)
        {
            nuint inputLength = (uint)input.Length;
            nuint outputLength = (uint)output.Length;
            fixed (byte* pInput = input, pOutput = output)
            {
                return LzhamNative.lzham_compress2(state, pInput, ref inputLength, pOutput, ref outputLength, flushType);
            }
        }

        /// <inheritdoc cref="LzhamNative.lzham_compress_memory(ref CompressionParameters, byte*, ref nuint, byte*, nuint, ref uint)"/>
        ///
        public static unsafe CompressStatus CompressMemory(CompressionParameters parameters, ReadOnlySpan<byte> input, Span<byte> output, ref uint adler32)
        {
            parameters.Initialize();
            nuint inputLength = (uint)input.Length;
            nuint outputLength = (uint)output.Length;
            fixed (byte* pInput = input, pOutput = output)
            {
                return LzhamNative.lzham_compress_memory(ref parameters, pOutput, ref outputLength, pInput, inputLength, ref adler32);
            }
        }

        /// <inheritdoc cref="LzhamNative.lzham_compress_deinit(CompressionHandle)"/>
        ///
        public static uint CompressDeinit(CompressionHandle state)
        {
            return LzhamNative.lzham_compress_deinit(state);
        }

        #endregion Compression

        #region Decompression

        /// <param name="parameters">The decompression parameters to use for the decompressor.</param>
        /// <inheritdoc cref="LzhamNative.lzham_decompress_init(ref DecompressionParameters)"/>
        public static DecompressionHandle DecompressInit(DecompressionParameters parameters)
        {
            parameters.Initialize();
            return LzhamNative.lzham_decompress_init(ref parameters);
        }

        /// <param name="parameters"><inheritdoc path="//param[1]" cref="DecompressInit(DecompressionParameters)"/></param>
        /// <inheritdoc cref="LzhamNative.lzham_decompress_reinit(DecompressionHandle, ref DecompressionParameters)"/>
        public static DecompressionHandle DecompressReinit(DecompressionHandle state, DecompressionParameters parameters)
        {
            parameters.Initialize();
            return LzhamNative.lzham_decompress_reinit(state, ref parameters);
        }

        /// <inheritdoc cref="LzhamNative.lzham_decompress(DecompressionHandle, byte*, ref nuint, byte*, ref nuint, byte)"/>
        ///
        public static unsafe DecompressStatus Decompress(DecompressionHandle state, ReadOnlySpan<byte> input, Span<byte> output, bool noMoreInputBytes)
        {
            nuint inputLength = (uint)input.Length;
            nuint outputLength = (uint)output.Length;
            fixed (byte* pInput = input, pOutput = output)
            {
                return LzhamNative.lzham_decompress(state, pInput, ref inputLength, pOutput, ref outputLength, noMoreInputBytes ? 1 : 0);
            }
        }

        /// <inheritdoc cref="LzhamNative.lzham_decompress_memory(ref DecompressionParameters, byte*, ref nuint, byte*, nuint, ref uint)"/>
        ///
        public static unsafe DecompressStatus DecompressMemory(DecompressionParameters parameters, ReadOnlySpan<byte> input, Span<byte> output, ref uint adler32)
        {
            parameters.Initialize();
            nuint inputLength = (uint)input.Length;
            nuint outputLength = (uint)output.Length;
            fixed (byte* pInput = input, pOutput = output)
            {
                return LzhamNative.lzham_decompress_memory(ref parameters, pOutput, ref outputLength, pInput, inputLength, ref adler32);
            }
        }

        /// <inheritdoc cref="LzhamNative.lzham_decompress_deinit(DecompressionHandle)"/>
        ///
        public static uint DecompressDeinit(DecompressionHandle state)
        {
            return LzhamNative.lzham_decompress_deinit(state);
        }

        #endregion Decompression
    }
}