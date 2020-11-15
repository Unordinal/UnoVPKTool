using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnoVPKTool.Extensions;

namespace UnoVPKTool.VPK
{
    /// <summary>
    /// Represents a block of entries in a VPK directory file AND to a single file that resides in a VPK archive.
    /// </summary>
    public class DirectoryEntryBlock : IBinaryWritable
    {
        /// <summary>
        /// Marks the end of a block of entries.
        /// </summary>
        public const ushort Terminator = ushort.MaxValue;

        /// <summary>
        /// A 32-bit CRC of the archive's data.
        /// </summary>
        public uint CRC { get; set; }

        /// <summary>
        /// The number of bytes contained in the directory file.
        /// </summary>
        public ushort PreloadBytes { get; set; }

        /// <summary>
        /// A zero-based index of the archive this entry's data is contained in.
        /// </summary>
        public ushort ArchiveIndex { get; set; }

        /// <summary>
        /// The entries contained within this block.
        /// </summary>
        public IList<DirectoryEntry> Entries { get; set; }

        /// <summary>
        /// The path of this file in the VPK archive. (ex: <c>resource\localization\base_english.txt</c>)
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets the total uncompressed size of this block by adding the uncompressed size of all entries.
        /// </summary>
        public ulong TotalUncompressedSize
        {
            get
            {
                ulong sum = 0; // I could use .Sum() and cast to long and then back to ulong...
                foreach (var entry in Entries) sum += entry.UncompressedSize;
                return sum;
            }
        }

        /// <summary>
        /// Gets the total compressed size of this block by adding the compressed size of all entries.
        /// </summary>
        public ulong TotalCompressedSize
        {
            get
            {
                ulong sum = 0; // I could use .Sum() and cast to long and then back to ulong...
                foreach (var entry in Entries) sum += entry.CompressedSize;
                return sum;
            }
        }

        /// <summary>
        /// Initializes a new block of entries using the given <see cref="BinaryReader"/> and paths.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="filePath">The path of this file in the VPK archive. (ex: <c>resource/localization/base_english.txt</c>)</param>
        public DirectoryEntryBlock(BinaryReader reader, string filePath)
        {
            FilePath = filePath;
            CRC = reader.ReadUInt32();
            PreloadBytes = reader.ReadUInt16();
            ArchiveIndex = reader.ReadUInt16();
            Entries = reader.ReadEntries().ToList();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(CRC);
            writer.Write(PreloadBytes);
            writer.Write(ArchiveIndex);
            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                writer.Write(entry);

                if (i == Entries.Count - 1)
                    writer.Write(Terminator);
                else
                    writer.Write(new byte[] { 0, 0 });
            }
        }

        public override string ToString()
        {
            return
                $"{nameof(FilePath)}: {FilePath}\n" +
                $"{nameof(CRC)}: 0x{CRC:X4}\n" +
                $"{nameof(PreloadBytes)}: {PreloadBytes}\n" +
                $"{nameof(ArchiveIndex)}: {ArchiveIndex}\n" +
                $"Entry Count: {Entries.Count}";
        }
    }
}