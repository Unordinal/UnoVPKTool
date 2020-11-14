using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnoVPKTool.Extensions;
using UnoVPKTool.VPK;

namespace UnoVPKTool.Tests
{
    [TestClass]
    public class FileTests
    {
        private const string BasePath = @"..\..\..\"; // Path to base test project directory.
        private const string TestFilesPath = BasePath + @"TestFiles\";

        private static readonly IEnumerable<string> TestFiles = Directory.EnumerateFiles(TestFilesPath, "*.vpk", SearchOption.TopDirectoryOnly);
        private static readonly string TestFile = Path.Combine(TestFilesPath, "englishclient_frontend.bsp.pak000_dir.vpk");

        [TestMethod]
        public void SimpleReadTest()
        {
            using var reader = RetrieveReaderForFilePath(TestFile);
            DirectoryFile testFile = new DirectoryFile(reader);

            Debug.WriteLine(testFile.ToString());
            Assert.IsTrue(testFile.Magic == DirectoryFile.ExpectedMagic);
            Assert.IsTrue(DirectoryFile.VersionIsSupported(testFile.Version));
        }

        [TestMethod]
        public void GetAllDataIDs()
        {
            using var reader = RetrieveReaderForFilePath(TestFile);
            DirectoryFile testFile = new DirectoryFile(reader);
            List<DirectoryEntry> allEntries = new List<DirectoryEntry>();
            foreach (var block in testFile.EntryBlocks)
            {
                foreach (var entry in block.Entries)
                {
                    //if (entry.DataID != 257)
                    //{
                        /*Debug.WriteLine($"{Path.GetFileName(block.FilePath)}: {entry.DataID} ({block.ArchiveIndex}, {entry.Offset})");
                        Debug.WriteLine($"{entry}");*/
                        //Debug.WriteLine($"{Path.GetFileName(block.FilePath)}");
                        //Debug.WriteLine(entry);
                    //}
                    allEntries.Add(entry);
                }
            }

            foreach (var e in allEntries.GroupBy(e => e.Unknown1).Select(g => g.First())) Debug.WriteLine(e);
        }

        [TestMethod]
        public void DataExtractionTest()
        {
            DirectoryFile file;
            using (var stream = File.OpenRead(TestFile))
            {
                using var reader = new BinaryReader(stream);
                file = new DirectoryFile(reader);
            }

            Assert.IsNotNull(file);
            foreach (var block in file.EntryBlocks)
            {
                string absolute = Path.GetFullPath(TestFilesPath);
                var data = Extractor.GetDataFromBlock(absolute, Path.GetFileName(TestFile), block);
                Directory.CreateDirectory(Path.Combine(absolute, "extracted"));
                File.WriteAllBytes(Path.Combine(absolute, "extracted", Path.GetFileName(block.FilePath!)), data);
            }
        }

        [TestMethod]
        public void DecompTest()
        {
            using var stream = File.OpenRead(TestFile);
            using var reader = new BinaryReader(stream);
            var file = new DirectoryFile(reader);

            var textEntryBlock = file.EntryBlocks.First((b) => b.FilePath?.EndsWith(".txt") == true);
            var textEntry = textEntryBlock.Entries.First();
            Debug.WriteLine(textEntryBlock.FilePath);
            Debug.WriteLine("Entries: " + textEntryBlock.Entries.Count);
            string pakArchiveName = TestFile.Replace("english", "").Replace("_dir", "_" + textEntryBlock.ArchiveIndex.ToString("000"));

            using var archiveStream = File.OpenRead(pakArchiveName);
            using var archiveReader = new BinaryReader(archiveStream);
            archiveStream.Seek((long)textEntry.Offset, SeekOrigin.Begin);

            byte[] compData = archiveReader.ReadBytes((int)textEntry.CompressedSize);
            Debug.WriteLine(string.Join("", compData.Select(s => s.ToString("X2"))));

            byte[] dcmpData = Lzham.DecompressMemory(compData, textEntry.UncompressedSize);
            Debug.WriteLine(string.Join("", dcmpData));

            File.WriteAllBytes(TestFilesPath + Path.GetFileName(textEntryBlock.FilePath) + ".dcmp", dcmpData);
        }

        /*[TestMethod]
        public void OutputFileTreeTest()
        {
            foreach (var file in TestFiles)
            {
                using var stream = File.OpenRead(file);
                using var reader = new BinaryReader(stream);
                VPK.File vpk = new VPK.File(reader);

                File.WriteAllText(file + ".tree.txt", vpk.Tree.RootNode.PrintTree());
            }
        }*/

        [TestMethod]
        public void TreeTest()
        {
            using var reader = RetrieveReaderForFilePath(TestFile);

            reader.BaseStream.Seek(0x10, SeekOrigin.Begin); // Beginning of full tree
            //reader.BaseStream.Seek(0x26A, SeekOrigin.Begin); // Beginning of txt
            Tree tree = new Tree(reader);

            tree.RootNode.Traverse((s) => Debug.WriteLine(s.ToString() + "\n"));
            //Debug.WriteLine(tree.RootNode.PrintTree());
            Assert.IsTrue(tree.RootNode.Header == "root");
        }

        [TestMethod]
        public void ReadEntryTest()
        {
            using var reader = RetrieveReaderForFilePath(TestFile);

            reader.BaseStream.Seek(0x39, SeekOrigin.Begin);
            DirectoryEntry testEntry = reader.ReadEntry();

            Debug.WriteLine(testEntry.ToString());
            Debug.WriteLine($"Length w/ Suffix: {Utils.GetBytesReadable(testEntry.UncompressedSize)}");
        }

        [TestMethod]
        public void ReadFileNames()
        {
            foreach (var file in TestFiles)
            {
                using var reader = RetrieveReaderForFilePath(file);
                DirectoryFile vpk = new DirectoryFile(reader);

                vpk.Tree.RootNode.Traverse((n) =>
                {
                    if (n.EntryBlock is not null)
                        Debug.WriteLine(n.EntryBlock.FilePath);
                });
            }
        }

        private BinaryReader RetrieveReaderForFilePath(string path)
        {
            var stream = File.OpenRead(path);
            var reader = new BinaryReader(stream);
            return reader;
        }
    }
}