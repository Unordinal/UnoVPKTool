using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoVPKTool.VPK
{
    public static class Extractor
    {
        /// <summary>
        /// Decompresses and returns the bytes of each entry in the given <see cref="DirectoryEntryBlock"/>.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="directoryPath"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public static byte[] GetDataFromBlock(string basePath, string directoryPath, DirectoryEntryBlock block)
        {
            string archivePath = GetArchivePathFromDirectoryPath(basePath, directoryPath, block.ArchiveIndex);
            using var fileStream = File.Open(archivePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            List<byte[]> decompData = new List<byte[]>();
            foreach (var entry in block.Entries)
            {
                fileStream.Seek((long)entry.Offset, SeekOrigin.Begin);
                byte[] buffer = reader.ReadBytes((int)entry.CompressedSize);
                byte[] decompressed;
                if (entry.CompressedSize != entry.UncompressedSize)
                {
                    decompressed = Lzham.DecompressMemory(buffer, entry.UncompressedSize);
                }
                else
                {
                    decompressed = buffer;
                }

                decompData.Add(decompressed);
            }

            return decompData.SelectMany(x => x).ToArray();
        }
        
        /// <summary>
        /// Gets a path to a VPK archive using the given base path, directory name and archive index.
        /// </summary>
        /// <param name="basePath">The base path where the VPK directory and archives reside.</param>
        /// <param name="vpkDirectoryName">The name of the VPK directory. (ex: <c>englishclient_frontend.bsp.pak000_dir.vpk</c>)</param>
        /// <param name="vpkArchiveIndex">The index of the VPK archive to use.</param>
        /// <returns></returns>
        private static string GetArchivePathFromDirectoryPath(string basePath, string vpkDirectoryName, ushort vpkArchiveIndex)
        {
            string pathWithoutLoc = StripLocalizationFromDirectoryPath(vpkDirectoryName);
            string archiveName = pathWithoutLoc.Replace("_dir", $"_{vpkArchiveIndex:000}");

            return Path.Combine(basePath, archiveName);
        }

        private static string StripLocalizationFromDirectoryPath(string directoryPath)
        {
            var localizations = Enum.GetNames<VPKLocalization>();
            string value = "";
            bool startsWith = localizations.Any((e) => directoryPath.StartsWith(value = e, StringComparison.OrdinalIgnoreCase));
            if (startsWith)
            {
                directoryPath = directoryPath[value.Length..];
            }

            return directoryPath;
        }
    }
}
