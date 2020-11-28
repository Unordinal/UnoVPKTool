﻿using System;
using LzhamWrapper.Decompression;

namespace UnoVPKTool
{
    public static class Lzham
    {
        /// <summary>
        /// The size of the LZHAM dictionary used to compress Apex Legends VPK archives.
        /// </summary>
        public const uint ApexDictSize = 20;

        public static byte[] DecompressMemory(byte[] compressedBytes, ulong uncompressedSize)
        {
            IntPtr compressedLength = new IntPtr((uint)compressedBytes.Length);

            byte[] decompressedBytes = new byte[uncompressedSize];
            IntPtr decompressedLength = new IntPtr((long)uncompressedSize);

            uint adler32 = 0;

            var parameters = new DecompressionParameters { DictionarySize = ApexDictSize, Flags = DecompressionFlags.OutputUnbuffered };
            parameters.Initialize();

            var result = LzhamWrapper.Lzham.Decompression.DecompressMemory(parameters, compressedBytes, decompressedBytes, ref adler32);
            if (result != DecompressStatus.Success)
            {
                throw new Exception("Lzham.DecompressMemory failed. Status: " + result.ToString());
            }
            if (decompressedLength.ToInt64() != (long)uncompressedSize)
            {
                throw new Exception($"Data length mismatch: {decompressedLength} vs {uncompressedSize}");
            }

            return decompressedBytes;
        }
    }
}
