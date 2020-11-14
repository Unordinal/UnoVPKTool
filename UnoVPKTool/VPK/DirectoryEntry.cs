using System.IO;

namespace UnoVPKTool.VPK
{
    /// <summary>
    /// Represents an entry in a VPK directory file. This contains information about the file stored in the VPK archive, such as the offset and size of the data.
    /// </summary>
    public class DirectoryEntry : IBinaryWritable
    {
        /// <summary>
        /// The size of an entry, in bytes.
        /// </summary>
        public const int ConstSize = 30;

        /// <summary>
        /// The entry data type ID. (?)
        /// <br/>
        /// Usually 257.
        /// </summary>
        public uint DataID { get; set; }

        /// <summary>
        /// Unknown. Flags? Known values: 0, 16(is there a 16?)
        /// </summary>
        public ushort Unknown1 { get; set; }

        /// <summary>
        /// Offset of the data within the archive VPK.
        /// </summary>
        public ulong Offset { get; set; }

        /// <summary>
        /// The size of the compressed data, in bytes.
        /// </summary>
        public ulong CompressedSize { get; set; }

        /// <summary>
        /// The size of the uncompressed data, in bytes.
        /// </summary>
        public ulong UncompressedSize { get; set; }

        /// <summary>
        /// Gets whether the data this entry points to is compressed.
        /// </summary>
        public bool IsCompressed => CompressedSize != UncompressedSize;

        /// <summary>
        /// Initializes a new entry using a <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The reader to read values from.</param>
        public DirectoryEntry(BinaryReader reader)
        {
            DataID = reader.ReadUInt32();
            Unknown1 = reader.ReadUInt16();
            Offset = reader.ReadUInt64();
            CompressedSize = reader.ReadUInt64();
            UncompressedSize = reader.ReadUInt64();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(DataID);
            writer.Write(Unknown1);
            writer.Write(Offset);
            writer.Write(CompressedSize);
            writer.Write(UncompressedSize);
        }

        public override string ToString()
        {
            return
                $"{nameof(DataID)}: {DataID}\n" +
                $"{nameof(Unknown1)}: {Unknown1}\n" +
                $"{nameof(Offset)}: {Offset}\n" +
                $"{nameof(CompressedSize)}: {CompressedSize}\n" +
                $"{nameof(UncompressedSize)}: {UncompressedSize}";
        }
    }
}