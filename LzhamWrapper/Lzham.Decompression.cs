using System;
using LzhamWrapper.Decompression;

namespace LzhamWrapper
{
    public static partial class Lzham
    {
        public static class Decompression
        {
            /// <param name="parameters">The decompression parameters to use for the decompressor.</param>
            /// <inheritdoc cref="LzhamNative.Decompression.Init(ref DecompressionParameters)"/>
            public static DecompressionHandle Init(DecompressionParameters parameters)
            {
                parameters.Initialize();
                return LzhamNative.Decompression.Init(ref parameters);
            }

            /// <param name="parameters"><inheritdoc path="//param[1]" cref="LzhamNative.Decompression.Reinit(DecompressionHandle, ref DecompressionParameters)"/></param>
            /// <inheritdoc cref="LzhamNative.Decompression.Reinit(DecompressionHandle, ref DecompressionParameters)"/>
            public static DecompressionHandle Reinit(DecompressionHandle state, DecompressionParameters parameters)
            {
                parameters.Initialize();
                return LzhamNative.Decompression.Reinit(state, ref parameters);
            }

            /// <inheritdoc cref="LzhamNative.Decompression.Decompress(DecompressionHandle, byte*, ref nuint, byte*, ref nuint, byte)"/>
            ///
            public static unsafe DecompressStatus Decompress(DecompressionHandle state, ReadOnlySpan<byte> input, Span<byte> output, bool noMoreInputBytes)
            {
                return Decompress(state, input, out _, output, out _, noMoreInputBytes);
            }

            /// <param name="bytesRead">The number of bytes that were read from <paramref name="input"/>.</param>
            /// <param name="bytesWritten">The number of bytes that were written to <paramref name="output"/>.</param>
            /// <inheritdoc cref="LzhamNative.Decompression.Decompress(DecompressionHandle, byte*, ref nuint, byte*, ref nuint, byte)"/>
            public static unsafe DecompressStatus Decompress(DecompressionHandle state, ReadOnlySpan<byte> input, out int bytesRead, Span<byte> output, out int bytesWritten, bool noMoreInputBytes)
            {
                DecompressStatus status;
                nuint inputLength = (uint)input.Length;
                nuint outputLength = (uint)output.Length;
                fixed (byte* pInput = input, pOutput = output)
                {
                    status = LzhamNative.Decompression.Decompress(state, pInput, ref inputLength, pOutput, ref outputLength, noMoreInputBytes ? 1 : 0);
                }
                bytesRead = (int)inputLength;
                bytesWritten = (int)outputLength;
                return status;
            }

            /// <inheritdoc cref="LzhamNative.Decompression.DecompressMemory(ref DecompressionParameters, byte*, ref nuint, byte*, nuint, ref uint)"/>
            ///
            public static unsafe DecompressStatus DecompressMemory(DecompressionParameters parameters, ReadOnlySpan<byte> input, Span<byte> output, ref uint adler32)
            {
                return DecompressMemory(parameters, input, out _, output, out _, ref adler32);
            }

            /// <inheritdoc path="//param" cref="Decompress(DecompressionHandle, ReadOnlySpan{byte}, out int, Span{byte}, out int, bool)"/>
            /// <inheritdoc cref="LzhamNative.Decompression.DecompressMemory(ref DecompressionParameters, byte*, ref nuint, byte*, nuint, ref uint)"/>
            public static unsafe DecompressStatus DecompressMemory(DecompressionParameters parameters, ReadOnlySpan<byte> input, out int bytesRead, Span<byte> output, out int bytesWritten, ref uint adler32)
            {
                parameters.Initialize();
                DecompressStatus status;
                nuint inputLength = (uint)input.Length;
                nuint outputLength = (uint)output.Length;
                fixed (byte* pInput = input, pOutput = output)
                {
                    status = LzhamNative.Decompression.DecompressMemory(ref parameters, pOutput, ref outputLength, pInput, inputLength, ref adler32);
                }
                bytesRead = (int)inputLength;
                bytesWritten = (int)outputLength;
                return status;
            }

            /// <inheritdoc cref="LzhamNative.Decompression.Deinit(DecompressionHandle)"/>
            ///
            public static uint Deinit(DecompressionHandle state)
            {
                return LzhamNative.Decompression.Deinit(state);
            }
        }
    }
}