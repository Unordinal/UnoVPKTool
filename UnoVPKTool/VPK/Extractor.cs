using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UnoVPKTool.VPK
{
    public static class Extractor
    {
        /// <summary>
        /// Decompresses and returns the bytes of the given <see cref="DirectoryEntryBlock"/>.
        /// </summary>
        /// <param name="vpkDirectoryFilePath">The path to the VPK directory file.</param>
        /// <param name="block">The block to extract.</param>
        /// <param name="vpkArchiveDirectory">The directory where the VPK archives are stored. If empty, uses the base VPK directory.</param>
        /// <returns></returns>
        public static byte[] ExtractBlock(string vpkDirectoryFilePath, DirectoryEntryBlock block, string vpkArchiveDirectory = "")
        {
            if (!File.Exists(vpkDirectoryFilePath)) throw new FileNotFoundException("The given VPK directory file was not found: " + vpkDirectoryFilePath);

            string archivePath = Utils.DirectoryPathToArchivePath(vpkDirectoryFilePath, block.ArchiveIndex, vpkArchiveDirectory);
            if (!File.Exists(archivePath)) throw new FileNotFoundException("Could not find a VPK archive at the given path: " + archivePath);

            using var fileStream = File.Open(archivePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            List<byte[]> decompData = new List<byte[]>();
            foreach (var entry in block.Entries)
            {
                fileStream.Seek((long)entry.Offset, SeekOrigin.Begin);

                byte[] buffer = reader.ReadBytes((int)entry.CompressedSize);
                byte[] decompressed;
                if (entry.IsCompressed)
                    decompressed = Lzham.DecompressMemory(buffer, entry.UncompressedSize);
                else
                    decompressed = buffer;

                decompData.Add(decompressed);
            }

            return decompData.SelectMany(x => x).ToArray();
        }

        /// <summary>
        /// Decompresses and returns the bytes of the given <see cref="DirectoryEntryBlock"/> asynchronously.
        /// </summary>
        /// <param name="vpkDirectoryFilePath">The path to the VPK directory file.</param>
        /// <param name="block">The block to extract.</param>
        /// <param name="vpkArchiveDirectory">The directory where the VPK archives are stored. If empty, uses the base VPK directory.</param>
        /// <returns></returns>
        public static async Task<byte[]> ExtractBlockAsync(string vpkDirectoryFilePath, DirectoryEntryBlock block, string vpkArchiveDirectory = "")
        {
            if (!File.Exists(vpkDirectoryFilePath)) throw new FileNotFoundException("The given VPK directory file was not found: " + vpkDirectoryFilePath);

            string archivePath = Utils.DirectoryPathToArchivePath(vpkDirectoryFilePath, block.ArchiveIndex, vpkArchiveDirectory);
            if (!File.Exists(archivePath)) throw new FileNotFoundException("Could not find a VPK archive at the given path: " + archivePath);

            using var fileStream = File.Open(archivePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            List<byte[]> decompData = new List<byte[]>();
            foreach (var entry in block.Entries)
            {
                fileStream.Seek((long)entry.Offset, SeekOrigin.Begin);

                byte[] buffer = reader.ReadBytes((int)entry.CompressedSize);
                byte[] decompressed;
                if (entry.IsCompressed)
                    decompressed = Lzham.DecompressMemory(buffer, entry.UncompressedSize);
                else
                    decompressed = buffer;

                decompData.Add(decompressed);
            }

            return decompData.SelectMany(x => x).ToArray();
        }

        /// <summary>
        /// Reads the data of a <see cref="DirectoryEntry"/> from a VPK archive.
        /// </summary>
        /// <returns></returns>
        public static byte[] GetEntryData(string vpkDirectoryFilePath, DirectoryEntry entry, string vpkArchiveDirectory = "")
        {
            return null;
        }
    }
}