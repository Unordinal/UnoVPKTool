using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnoVPKTool.VPK;

namespace UnoVPKTool.Tests
{
    [TestClass]
    public class FileTests
    {
        private const string BasePath = @"..\..\..\"; // Path to base test project directory.
        private const string TestFilesPath = BasePath + @"TestFiles\";

        private static readonly IEnumerable<string> TestFiles = Directory.EnumerateFiles(TestFilesPath, "*_dir.vpk", SearchOption.TopDirectoryOnly);
        private static readonly string TestFile = Path.Combine(TestFilesPath, "englishclient_frontend.bsp.pak000_dir.vpk");
        private static readonly string TestArchive = Path.Combine(TestFilesPath, "client_frontend.bsp.pak000_000.vpk");
        private static readonly string ExtractPath = Path.Combine(TestFilesPath, "extracted");

        [TestMethod]
        public void SimpleReadTest()
        {
            _ = new DirectoryFile(TestFile);
        }

        [TestMethod]
        public void TestPerfTest()
        {
            byte[] buffer = new byte[1000];
            Random rnd = new Random();
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)rnd.Next(byte.MinValue, byte.MaxValue);
            }

            Console.WriteLine("WriteAllBytes");
            Benchmarker.BenchmarkTime(() =>
            {
                //File.WriteAllBytes(TestFilesPath + "test.test", buffer);
                using var fs = File.Create(TestFilesPath + "test.test", buffer.Length, FileOptions.SequentialScan);
                fs.Write(buffer);
            }, 500);
            Console.WriteLine("");

            Console.WriteLine("FS Seek and Read");
            Benchmarker.BenchmarkTime(() =>
            {
                using var fs = File.Open(TestArchive, FileMode.Open, FileAccess.Read, FileShare.Read);
                int offset = rnd.Next(0, 100000);
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Read(buffer);
            }, 500);
            Console.WriteLine("");

            using var mmf = MemoryMappedFile.CreateFromFile(TestArchive);
            using var mmvs = mmf.CreateViewStream();
            Console.WriteLine("MMVS Seek and Read");
            Benchmarker.BenchmarkTime(() =>
            {
                int offset = rnd.Next(0, 100000);
                mmvs.Seek(offset, SeekOrigin.Begin);
                mmvs.Read(buffer);
            }, 500);
            Console.WriteLine("");

            using var mmfo = MemoryMappedFile.CreateFromFile(TestFilesPath + "test.test", FileMode.Create, null, 1000);
            using var mmvso = mmf.CreateViewStream();
            Console.WriteLine("Seek and Read, Write");
            Benchmarker.BenchmarkTime(() =>
            {
                int offset = rnd.Next(0, 100000);
                mmvs.Seek(offset, SeekOrigin.Begin);
                mmvs.Read(buffer);
                mmvso.Seek(0, SeekOrigin.Begin);
                mmvso.Write(buffer);
            }, 500);
            Console.WriteLine("");
        }

        [TestMethod]
        public void TestExtractorExtractAll()
        {
            var file = new DirectoryFile(TestFile);
            var fullDir = Path.Combine(ExtractPath, Path.GetFileNameWithoutExtension(file.FilePath) + Path.DirectorySeparatorChar);
            Directory.CreateDirectory(fullDir);

            using var extractor = new Extractor(file);
            extractor.ExtractAll(fullDir);
        }
        
        [TestMethod]
        public async Task TestExtractorExtractAllAsync()
        {
            var file = new DirectoryFile(TestFile);
            var fullDir = Path.Combine(ExtractPath, Path.GetFileNameWithoutExtension(file.FilePath) + Path.DirectorySeparatorChar);
            Directory.CreateDirectory(fullDir);

            int entriesToRead = file.EntryBlocks.Sum(b => b.Entries.Count);
            int entriesToDecompress = file.EntryBlocks.SelectMany(b => b.Entries.Where(e => e.IsCompressed)).Count();
            int entriesRead = 0;
            int entriesDecompressed = 0;
            object _lock = new object();

            void func(EntryOperation e)
            {
                lock (_lock)
                {
                    switch (e.OperationPerformed)
                    {
                        case EntryOperation.ProcessType.Read:
                            entriesRead++;
                            Console.WriteLine($"Read: {entriesRead}/{entriesToRead}");
                            break;
                        case EntryOperation.ProcessType.Decompress:
                            entriesDecompressed++;
                            Console.WriteLine($"Decompressed: {entriesDecompressed}/{entriesToDecompress}");
                            break;
                        default:
                            break;
                    }
                }
            }
            Progress<EntryOperation> prog = new Progress<EntryOperation>(func);

            using var extractor = new Extractor(file);
            var task = extractor.ExtractAllAsync(fullDir, prog);
            await task;
        }
        
        [TestMethod]
        public void TestExtractorAllFilesExtractAll()
        {
            foreach (var f in TestFiles)
            {
                var file = new DirectoryFile(f);
                var fullDir = Path.Combine(ExtractPath, Path.GetFileNameWithoutExtension(file.FilePath) + Path.DirectorySeparatorChar);
                Directory.CreateDirectory(fullDir);

                using var extractor = new Extractor(file);
                extractor.ExtractAll(fullDir);
            }
        }

        [TestMethod]
        public void TestTemplate()
        {
            var file = new DirectoryFile(TestFile);
            var fullDir = Path.Combine(ExtractPath, Path.GetFileNameWithoutExtension(file.FilePath) + Path.DirectorySeparatorChar);
            Directory.CreateDirectory(fullDir);
            using var extractor = new Extractor(file);
        }
    }
}