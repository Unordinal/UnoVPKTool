using System;
using LzhamWrapper.Compression;

namespace LzhamWrapper
{
    public static partial class Lzham
    {
        public static class Compression
        {
            /// <param name="parameters">The compression parameters to use for the compressor.</param>
            /// <inheritdoc cref="LzhamNative.Compression.Init(ref CompressionParameters)"/>
            public static CompressionHandle Init(CompressionParameters parameters)
            {
                parameters.Initialize();
                return LzhamNative.Compression.Init(ref parameters);
            }

            /// <inheritdoc cref="LzhamNative.Compression.Reinit(CompressionHandle)"/>
            ///
            public static CompressionHandle Reinit(CompressionHandle state)
            {
                return LzhamNative.Compression.Reinit(state);
            }

            /// <inheritdoc cref="LzhamNative.Compression.Compress(CompressionHandle, byte*, ref nuint, byte*, ref nuint, byte)"/>
            ///
            public static unsafe CompressStatus Compress(CompressionHandle state, ReadOnlySpan<byte> input, Span<byte> output, bool noMoreInputBytes)
            {
                nuint inputLength = (uint)input.Length;
                nuint outputLength = (uint)output.Length;
                fixed (byte* pInput = input, pOutput = output)
                {
                    return LzhamNative.Compression.Compress(state, pInput, ref inputLength, pOutput, ref outputLength, noMoreInputBytes ? 1 : 0);
                }
            }

            /// <param name="bytesRead">The number of bytes that were read from <paramref name="input"/>.</param>
            /// <param name="bytesWritten">The number of bytes that were written to <paramref name="output"/>.</param>
            /// <inheritdoc cref="Compress(CompressionHandle, ReadOnlySpan{byte}, Span{byte}, bool)"/>
            public static unsafe CompressStatus Compress(CompressionHandle state, ReadOnlySpan<byte> input, out int bytesRead, Span<byte> output, out int bytesWritten, bool noMoreInputBytes)
            {
                CompressStatus status;
                nuint inputLength = (uint)input.Length;
                nuint outputLength = (uint)output.Length;
                fixed (byte* pInput = input, pOutput = output)
                {
                    status = LzhamNative.Compression.Compress(state, pInput, ref inputLength, pOutput, ref outputLength, noMoreInputBytes ? 1 : 0);
                }
                bytesRead = (int)inputLength;
                bytesWritten = (int)outputLength;
                return status;
            }

            /// <inheritdoc cref="LzhamNative.Compression.Compress(CompressionHandle, byte*, ref nuint, byte*, ref nuint, Flush)"/>
            ///
            public static unsafe CompressStatus Compress(CompressionHandle state, ReadOnlySpan<byte> input, Span<byte> output, Flush flushType)
            {
                nuint inputLength = (uint)input.Length;
                nuint outputLength = (uint)output.Length;
                fixed (byte* pInput = input, pOutput = output)
                {
                    return LzhamNative.Compression.Compress(state, pInput, ref inputLength, pOutput, ref outputLength, flushType);
                }
            }

            /// <inheritdoc path="//param" cref="Compress(CompressionHandle, ReadOnlySpan{byte}, out int, Span{byte}, out int, bool)"/>
            /// <inheritdoc cref="Compress(CompressionHandle, ReadOnlySpan{byte}, Span{byte}, Flush)"/>
            public static unsafe CompressStatus Compress(CompressionHandle state, ReadOnlySpan<byte> input, out int bytesRead, Span<byte> output, out int bytesWritten, Flush flushType)
            {
                CompressStatus status;
                nuint inputLength = (uint)input.Length;
                nuint outputLength = (uint)output.Length;
                fixed (byte* pInput = input, pOutput = output)
                {
                    status = LzhamNative.Compression.Compress(state, pInput, ref inputLength, pOutput, ref outputLength, flushType);
                }
                bytesRead = (int)inputLength;
                bytesWritten = (int)outputLength;
                return status;
            }

            /// <inheritdoc cref="LzhamNative.Compression.CompressMemory(ref CompressionParameters, byte*, ref nuint, byte*, nuint, ref uint)"/>
            ///
            public static unsafe CompressStatus CompressMemory(CompressionParameters parameters, ReadOnlySpan<byte> input, Span<byte> output, ref uint adler32)
            {
                parameters.Initialize();
                nuint inputLength = (uint)input.Length;
                nuint outputLength = (uint)output.Length;
                fixed (byte* pInput = input, pOutput = output)
                {
                    return LzhamNative.Compression.CompressMemory(ref parameters, pOutput, ref outputLength, pInput, inputLength, ref adler32);
                }
            }

            /// <inheritdoc path="//param" cref="Compress(CompressionHandle, ReadOnlySpan{byte}, out int, Span{byte}, out int, bool)"/>
            /// <inheritdoc cref="LzhamNative.Compression.CompressMemory(ref CompressionParameters, byte*, ref nuint, byte*, nuint, ref uint)"/>
            public static unsafe CompressStatus CompressMemory(CompressionParameters parameters, ReadOnlySpan<byte> input, out int bytesRead, Span<byte> output, out int bytesWritten, ref uint adler32)
            {
                parameters.Initialize();
                CompressStatus status;
                nuint inputLength = (uint)input.Length;
                nuint outputLength = (uint)output.Length;
                fixed (byte* pInput = input, pOutput = output)
                {
                    status = LzhamNative.Compression.CompressMemory(ref parameters, pOutput, ref outputLength, pInput, inputLength, ref adler32);
                }
                bytesRead = (int)inputLength;
                bytesWritten = (int)outputLength;
                return status;
            }

            /// <inheritdoc cref="LzhamNative.Compression.Deinit(CompressionHandle)"/>
            ///
            public static uint Deinit(CompressionHandle state)
            {
                return LzhamNative.Compression.Deinit(state);
            }
        }
    }
}