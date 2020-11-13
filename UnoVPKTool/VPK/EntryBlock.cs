using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnoVPKTool.Extensions;

namespace UnoVPKTool.VPK
{
    /// <summary>
    /// Represents a block of entries in a VPK directory file.
    /// </summary>
    public class EntryBlock : IBinaryWritable
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
        /// A zero-based index of the archive this entry's data is contained in. If 0x7FFF, the data follows the directory.
        /// </summary>
        public ushort ArchiveIndex { get; set; }

        /// <summary>
        /// The entries contained within this block.
        /// </summary>
        public IList<Entry> Entries { get; set; }

        /// <summary>
        /// The complete path of this file, ex: resource/localization/base_english.txt
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Initializes a new block of entries using the given <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public EntryBlock(BinaryReader reader)
        {
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
                $"{nameof(CRC)}: 0x{CRC:X4}\n" +
                $"{nameof(PreloadBytes)}: {PreloadBytes}\n" +
                $"{nameof(ArchiveIndex)}: {ArchiveIndex}\n" +
                $"Entry Count: {Entries.Count}";
        }
    }
}