using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnoVPKTool.Extensions;

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
            using var stream = File.OpenRead(TestFile);
            using var reader = new BinaryReader(stream);
            VPK.File testFile = new VPK.File(reader);

            Debug.WriteLine(testFile.ToString());
            Assert.IsTrue(testFile.Magic == VPK.File.ExpectedMagic);
            Assert.IsTrue(testFile.VersionMajor == VPK.File.SupportedVersionMajor);
            Assert.IsTrue(testFile.VersionMinor == VPK.File.SupportedVersionMinor);
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
            using var stream = File.OpenRead(TestFile);
            using var reader = new BinaryReader(stream);

            reader.BaseStream.Seek(0x10, SeekOrigin.Begin); // Beginning of full tree
            //reader.BaseStream.Seek(0x26A, SeekOrigin.Begin); // Beginning of txt
            VPK.Tree tree = new VPK.Tree(reader);

            tree.RootNode.Traverse((s) => Debug.WriteLine(s.ToString() + "\n"));
            //Debug.WriteLine(tree.RootNode.PrintTree());
            Assert.IsTrue(tree.RootNode.Header == "root");
        }

        [TestMethod]
        public void ReadEntryTest()
        {
            using var stream = File.OpenRead(TestFile);
            using var reader = new BinaryReader(stream);

            reader.BaseStream.Seek(0x39, SeekOrigin.Begin);
            VPK.Entry testEntry = reader.ReadEntry();

            Debug.WriteLine(testEntry.ToString());
            Debug.WriteLine($"Length w/ Suffix: {Utils.GetBytesReadable(testEntry.UncompressedSize)}");
        }

        [TestMethod]
        public void ReadFileNames()
        {
            foreach (var file in TestFiles)
            {
                using var stream = File.OpenRead(file);
                using var reader = new BinaryReader(stream);
                VPK.File vpk = new VPK.File(reader);

                vpk.Tree.RootNode.Traverse((n) =>
                {
                    if (n.EntryBlock is not null)
                        Debug.WriteLine(n.EntryBlock.FilePath);
                });
            }
        }
    }
}