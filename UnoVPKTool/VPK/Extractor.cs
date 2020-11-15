using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnoVPKTool.VPK
{
    public static class Extractor
    {
        /// <summary>
        /// Decompresses and returns the bytes of the given <see cref="DirectoryEntryBlock"/>.
        /// </summary>
        /// <param name="directoryFilePath">The path to the VPK directory file.</param>
        /// <param name="block">The block to extract.</param>
        /// <param name="archiveDirectory">The directory where the VPK archives are stored. If empty, uses the base VPK directory.</param>
        /// <returns></returns>
        public static byte[] ExtractBlock(string directoryFilePath, DirectoryEntryBlock block, string archiveDirectory = "")
        {
            if (!File.Exists(directoryFilePath)) throw new FileNotFoundException("The given VPK directory file was not found: " + directoryFilePath);

            string archivePath = Utils.DirectoryPathToArchivePath(directoryFilePath, block.ArchiveIndex, archiveDirectory);
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
    }
}