using System;
using System.Runtime.InteropServices;
using LzhamWrapper.Compression;
using LzhamWrapper.Decompression;

namespace LzhamWrapper
{
    public class Lzham
    {
        private const string LzhamDll = "lzham_x64.dll";

        public static uint GetVersion()
        {
            return lzham_get_version();
        }

        #region Compression

        public static unsafe CompressionHandle CompressInit(CompressionParameters parameters)
        {
            parameters.Initialize();
            return lzham_compress_init(ref parameters);
        }

        public static CompressionHandle CompressReinit(CompressionHandle state)
        {
            return lzham_compress_reinit(state);
        }

        public static unsafe CompressStatus Compress(CompressionHandle state, ReadOnlySpan<byte> inBuf, ref nuint inBufSize, Span<byte> outBuf, ref nuint outBufSize, bool noMoreInputBytesFlag)
        {
            if (inBufSize > (uint)inBuf.Length) throw new ArgumentOutOfRangeException(nameof(inBuf), "Offset + Size is larger than the length of the array.");
            if (outBufSize > (uint)outBuf.Length) throw new ArgumentOutOfRangeException(nameof(outBuf), "Offset + Size is larger than the length of the array.");

            fixed (byte* inBytes = inBuf, outBytes = outBuf)
            {
                return lzham_compress(state, inBytes, ref inBufSize, outBytes, ref outBufSize, noMoreInputBytesFlag ? 1 : 0);
            }
        }

        public static unsafe CompressStatus Compress2(CompressionHandle state, ReadOnlySpan<byte> inBuf, ref IntPtr inBufSize, int inBufOffset, Span<byte> outBuf, ref IntPtr outBufSize, int outBufOffset, Flush flushType)
        {
            if (inBufOffset + inBufSize.ToInt64() > inBuf.Length) throw new ArgumentOutOfRangeException(nameof(inBuf), "Offset + Size is larger than the length of the array.");
            if (outBufOffset + outBufSize.ToInt64() > outBuf.Length) throw new ArgumentOutOfRangeException(nameof(outBuf), "Offset + Size is larger than the length of the array.");

            fixed (byte* inBytes = inBuf, outBytes = outBuf)
            {
                return lzham_compress2(state, inBytes + inBufOffset, ref inBufSize, outBytes + outBufOffset, ref outBufSize, flushType);
            }
        }

        public static unsafe CompressStatus CompressMemory(CompressionParameters parameters, ReadOnlySpan<byte> inBuf, IntPtr inBufSize, int inBufOffset, Span<byte> outBuf, ref IntPtr outBufSize, int outBufOffset, ref uint adler32)
        {
            if (inBufOffset + inBufSize.ToInt64() > inBuf.Length) throw new ArgumentOutOfRangeException(nameof(inBuf), "Offset + Size is larger than the length of the array.");
            if (outBufOffset + outBufSize.ToInt64() > outBuf.Length) throw new ArgumentOutOfRangeException(nameof(outBuf), "Offset + Size is larger than the length of the array.");

            parameters.Initialize();
            fixed (byte* inBytes = inBuf, outBytes = outBuf)
            {
                return lzham_compress_memory(ref parameters, outBytes + outBufOffset, ref outBufSize, inBytes + inBufOffset, inBufSize, ref adler32);
            }
        }

        public static uint CompressDeinit(CompressionHandle state)
        {
            return lzham_compress_deinit(state);
        }

        #endregion Compression

        #region Decompression

        public static unsafe DecompressionHandle DecompressInit(DecompressionParameters parameters)
        {
            parameters.Initialize();
            return lzham_decompress_init(ref parameters);
        }

        public static unsafe DecompressionHandle DecompressReinit(DecompressionHandle state, DecompressionParameters parameters)
        {
            parameters.Initialize();
            return lzham_decompress_reinit(state, ref parameters);
        }

        public static unsafe DecompressStatus Decompress(DecompressionHandle state, ReadOnlySpan<byte> inBuf, ref nuint inCount, Span<byte> outBuf, ref nuint outCount, bool noMoreInputBytesFlag)
        {
            if (inCount > (uint)inBuf.Length) throw new ArgumentOutOfRangeException(nameof(inBuf), $"{nameof(inCount)} is larger than the length of {nameof(inBuf)}.");
            if (outCount > (uint)outBuf.Length) throw new ArgumentOutOfRangeException(nameof(outBuf), $"{nameof(outCount)} is larger than the length of {nameof(outBuf)}.");

            fixed (byte* inBytes = inBuf, outBytes = outBuf)
            {
                DecompressStatus result = lzham_decompress(state, inBytes, ref inCount, outBytes, ref outCount, noMoreInputBytesFlag ? 1 : 0);
                return result;
            }
        }

        public static unsafe DecompressStatus DecompressMemory(DecompressionParameters parameters, byte[] inBuf, ref IntPtr inBufSize, int inBufOffset, byte[] outBuf, ref IntPtr outBufSize, int outBufOffset, ref uint adler32)
        {
            if (inBufOffset + inBufSize.ToInt64() > inBuf.Length) throw new ArgumentOutOfRangeException(nameof(inBuf), "Offset + Size is larger than the length of the array.");
            if (outBufOffset + outBufSize.ToInt64() > outBuf.Length) throw new ArgumentOutOfRangeException(nameof(outBuf), "Offset + Size is larger than the length of the array.");

            parameters.Initialize();
            fixed (byte* inBytes = inBuf, outBytes = outBuf)
            {
                DecompressStatus result = lzham_decompress_memory(ref parameters, outBytes + outBufOffset, ref outBufSize, inBytes + inBufOffset, inBufSize, ref adler32);
                return result;
            }
        }

        public static uint DecompressDeinit(DecompressionHandle state)
        {
            return lzham_decompress_deinit(state);
        }

        #endregion Decompression

        #region P/Invoke

#pragma warning disable IDE1006 // Naming Styles

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint lzham_get_version();

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern CompressionHandle lzham_compress_init(ref CompressionParameters parameters);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern CompressionHandle lzham_compress_reinit(CompressionHandle state);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe CompressStatus lzham_compress(CompressionHandle state, byte* inBuf, ref nuint inLength, byte* outBuf, ref nuint outLength, byte noMoreInputBytesFlag);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe CompressStatus lzham_compress2(CompressionHandle state, byte* inBuf, ref IntPtr inBufSize, byte* outBuf, ref IntPtr outBufSize, Flush flushType);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe CompressStatus lzham_compress_memory(ref CompressionParameters parameters, byte* dstBuffer, ref IntPtr dstLength, byte* srcBuffer, IntPtr srcLength, ref uint adler32);

        /// <summary>
        /// Deinitializes a compressor, releasing all allocated memory.
        /// </summary>
        /// <param name="state"></param>
        /// <returns>Adler32 of source data (only valid on success).</returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint lzham_compress_deinit(CompressionHandle state);

        /// <summary>
        /// Initializes a decompressor.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe DecompressionHandle lzham_decompress_init(ref DecompressionParameters parameters);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe DecompressionHandle lzham_decompress_reinit(DecompressionHandle state, ref DecompressionParameters parameters);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe DecompressStatus lzham_decompress(DecompressionHandle state, byte* inBuf, ref nuint inLength, byte* outBuf, ref nuint outLength, byte noMoreInputBytesFlag);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe DecompressStatus lzham_decompress_memory(ref DecompressionParameters parameters, byte* dstBuffer, ref IntPtr dstLength, byte* srcBuffer, IntPtr srcLength, ref uint adler32);

        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint lzham_decompress_deinit(DecompressionHandle state);

#pragma warning restore IDE1006 // Naming Styles

        #endregion P/Invoke
    }
}