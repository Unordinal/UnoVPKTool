using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LzhamWrapper;
using LzhamWrapper.Compression;
using LzhamWrapper.Decompression;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnoVPKTool.Tests
{
    [TestClass]
    public class LzhamTests
    {
        public const int Iterations = 2500;
        public const string TestString = "Hello World! This is a test string to see if lzham methods are performed correctly.";
        public const int TestRawDataBufferSize = 256;

        public static readonly byte[] TestUncompressedData = Encoding.ASCII.GetBytes(TestString);
        public static readonly byte[] TestCompressedData =
            { 224, 0, 5, 37, 32, 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33, 32, 84, 104,
              105, 115, 32, 105, 115, 32, 97, 32, 116, 101, 115, 116, 32, 115, 116, 114, 105, 110, 103, 32,
              116, 111, 32, 115, 101, 101, 32, 105, 102, 32, 108, 122, 104, 97, 109, 32, 109, 101, 116, 104,
              111, 100, 115, 32, 97, 114, 101, 32, 112, 101, 114, 102, 111, 114, 109, 101, 100, 32, 99, 111,
              114, 114, 101, 99, 116, 108, 121, 46, 192, 207, 105, 30, 10 };
        public static readonly byte[] TestCompressedDataWithExtraData = TestCompressedData.Concat(TestCompressedData).ToArray();

        /*[TestMethod]
        public void TestCompression()
        {
            byte[] buffer = new byte[TestRawDataBufferSize];
            var compParams = new CompressionParameters { DictionarySize = 20 };
            using var lzham = new LzhamStream(new MemoryStream(buffer), compParams);

            lzham.Write(TestUncompressedData, 0, TestUncompressedData.Length);
            Debug.WriteLine(string.Join(' ', buffer));
        }

        [TestMethod]
        public void TestDecompression()
        {
            byte[] buffer = new byte[TestRawDataBufferSize];
            var decompParams = new DecompressionParameters { DictionarySize = 20, Flags = DecompressionFlags.OutputUnbuffered };
            using var lzham = new LzhamStream(new MemoryStream(TestCompressedData), decompParams);

            int bytesDecompressed = lzham.Read(buffer, 0, TestCompressedData.Length);
            buffer = buffer[0..bytesDecompressed];
            string decompString = Encoding.ASCII.GetString(buffer);

            Debug.WriteLine("[" + decompString + "]");
            Debug.WriteLine(string.Join(", ", buffer));

            buffer = new byte[TestRawDataBufferSize];
            lzham.Seek(0, SeekOrigin.Begin);
            bytesDecompressed = lzham.Read(buffer, 0, TestCompressedData.Length);
            buffer = buffer[0..bytesDecompressed];
            decompString = Encoding.ASCII.GetString(buffer);

            Debug.WriteLine("[" + decompString + "]");
            Debug.WriteLine(string.Join(", ", buffer));
        }*/

        [TestMethod]
        public void TestCompressionMemory()
        {
            byte[] buffer = new byte[TestRawDataBufferSize];
            var compParams = new CompressionParameters { DictionarySize = 20 };

            IntPtr outSize = new IntPtr(buffer.Length);
            uint adler32 = 0;
            var status = LzhamWrapper.Lzham.Compression.CompressMemory(compParams, TestUncompressedData, buffer, ref adler32);
            buffer = buffer[0..outSize.ToInt32()];
            string compString = Encoding.ASCII.GetString(buffer);

            Debug.WriteLine(status);
            Debug.WriteLine("[" + compString + "]");
            Debug.WriteLine(string.Join(", ", buffer));
        }

        [TestMethod]
        public void TestDecompression()
        {
            byte[] buffer = new byte[TestRawDataBufferSize];
            var decompParams = new DecompressionParameters { DictionarySize = 20 };
            var decompressor = new Decompressor(decompParams);

            DecompressStatus status;
            int bytesWritten = 0;
            for (int i = 0; i < Iterations; i++)
            {
                status = decompressor.Decompress(TestCompressedDataWithExtraData, out _, buffer, out bytesWritten, true);
            }
            string decompString = Encoding.ASCII.GetString(buffer.AsSpan(0, bytesWritten));

            Debug.WriteLine($"Input bytes: {TestCompressedDataWithExtraData.Length}");
            Debug.WriteLine($"Output bytes: {bytesWritten}");

            Assert.AreEqual(TestString, decompString, false);
        }

        [TestMethod]
        public void TestDecompressionMemory()
        {
            byte[] buffer = new byte[TestRawDataBufferSize];
            var decompParams = new DecompressionParameters { DictionarySize = 20 };
            var decompressor = new Decompressor(decompParams);

            DecompressStatus status;
            int bytesWritten = 0;
            uint adler32 = 0;
            for (int i = 0; i < Iterations; i++)
            {
                status = decompressor.DecompressMemory(TestCompressedDataWithExtraData, out _, buffer, out bytesWritten, ref adler32);
            }
            string decompString = Encoding.ASCII.GetString(buffer.AsSpan(0, bytesWritten));

            Debug.WriteLine($"Input bytes: {TestCompressedDataWithExtraData.Length}");
            Debug.WriteLine($"Output bytes: {bytesWritten}");

            Assert.AreEqual(TestString, decompString, false);
        }

        [TestMethod]
        public void TestDecompressionAllVsBlockPerformance()
        {
            const int Iterations = 800;

            byte[] buffer = new byte[TestRawDataBufferSize];
            var decompParams = new DecompressionParameters { DictionarySize = 20 };

            Stopwatch sw = Stopwatch.StartNew();

            using var lzham = new LzhamStream(new MemoryStream(TestCompressedData), decompParams);
            for (int i = 0; i < Iterations; i++)
            {
                lzham.Seek(0, SeekOrigin.Begin);
                lzham.Read(buffer, 0, TestCompressedData.Length);
            }
            sw.Stop();
            Debug.WriteLine("Block by block: " + sw.Elapsed);

            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                IntPtr inSize = new IntPtr(TestCompressedData.Length);
                IntPtr outSize = new IntPtr(buffer.Length);
                uint adler32 = 0;
                LzhamWrapper.Lzham.Decompression.DecompressMemory(decompParams, TestCompressedData, buffer, ref adler32);
            }
            sw.Stop();
            Debug.WriteLine("All: " + sw.Elapsed);
        }
    }
}