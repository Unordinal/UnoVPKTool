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

        private static readonly IEnumerable<string> TestFiles = Directory.EnumerateFiles(TestFilesPath, "*_dir.vpk", SearchOption.TopDirectoryOnly);
        private static readonly string TestFile = Path.Combine(TestFilesPath, "englishclient_frontend.bsp.pak000_dir.vpk");
        private static readonly string ExtractPath = Path.Combine(TestFilesPath, "extracted");

        [TestMethod]
        public void SimpleReadTest()
        {
            _ = new DirectoryFile(TestFile);
        }

        [TestMethod]
        public void ExtractAllFilesTest()
        {
            DirectoryFile file = new DirectoryFile(TestFile);
            var fullDir = Path.Combine(ExtractPath, Path.GetFileNameWithoutExtension(file.FilePath) + Path.DirectorySeparatorChar);
            Directory.CreateDirectory(fullDir);

            foreach (var block in file.EntryBlocks)
            {
                var path = Path.Combine(fullDir, block.FilePath);
                var blockData = Extractor.ExtractBlock(TestFile, block);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                File.WriteAllBytes(path, blockData);
            }
        }
        
        [TestMethod]
        public void ExtractAllFilesThruFileTest()
        {
            DirectoryFile file = new DirectoryFile(TestFile);
            var fullDir = Path.Combine(ExtractPath, Path.GetFileNameWithoutExtension(file.FilePath) + Path.DirectorySeparatorChar);
            Directory.CreateDirectory(fullDir);

            foreach (var block in file.EntryBlocks)
            {
                var path = Path.Combine(fullDir, block.FilePath);
                var blockData = file.ExtractBlock(block);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                File.WriteAllBytes(path, blockData);
            }
        }
        
        // Very slow, obviously.
        /*[TestMethod]
        public void ExtractAllFilesFromAllDirsTest()
        {
            foreach (var file in TestFiles)
            {
                string filePath = Path.GetFullPath(file);
                DirectoryFile dirFile = new DirectoryFile(filePath);
                var fullDir = Path.Combine(ExtractPath, Path.GetFileNameWithoutExtension(dirFile.FilePath) + Path.DirectorySeparatorChar);
                Directory.CreateDirectory(fullDir);

                foreach (var block in dirFile.EntryBlocks)
                {
                    var path = Path.Combine(fullDir, block.FilePath);
                    var blockData = Extractor.ExtractBlock(filePath, block);
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    File.WriteAllBytes(path, blockData);
                }
            }
        }*/

        private BinaryReader RetrieveReaderForFilePath(string path)
        {
            var stream = File.OpenRead(path);
            var reader = new BinaryReader(stream);
            return reader;
        }
    }
}