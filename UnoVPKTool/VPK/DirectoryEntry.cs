using System;
using System.IO;

namespace UnoVPKTool.VPK
{
    /// <summary>
    /// Represents an entry in a VPK directory file. This contains information about the file stored in the VPK archive, such as the offset and size of the data.
    /// </summary>
    public class DirectoryEntry : IBinaryWritable
    {
        /// <summary>
        /// The size of a <see cref="DirectoryEntry"/>, in bytes.
        /// </summary>
        public const int ConstSize = 30;

        /// <summary>
        /// The entry data type ID. (?)
        /// <br/>
        /// Usually 257.
        /// </summary>
        public EntryFlags EntryFlags { get; set; }

        /// <summary>
        /// If this entry is a .vtf texture, these are some flags relating to the texture contained in the entry in some way. This mostly relates to Titanfall 2, as the only .vtf Apex stores in the VPKs is a cloudmask in each map (which has this equal to 0x8).
        /// <br/>
        /// See the XML dcoumentation for <see cref="EntryTextureFlags"/> for info about each flag.
        /// </summary>
        public EntryTextureFlags TextureFlags { get; set; }

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
        /// Creates a new <see cref="DirectoryEntry"/>.
        /// </summary>
        /// <param name="archivePath">The path of the archive the data of this entry resides in.</param>
        public DirectoryEntry(BinaryReader reader)
        {
            EntryFlags = (EntryFlags)reader.ReadUInt32();
            TextureFlags = (EntryTextureFlags)reader.ReadUInt16();
            Offset = reader.ReadUInt64();
            CompressedSize = reader.ReadUInt64();
            UncompressedSize = reader.ReadUInt64();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((uint)EntryFlags);
            writer.Write((ushort)TextureFlags);
            writer.Write(Offset);
            writer.Write(CompressedSize);
            writer.Write(UncompressedSize);
        }

        public override string ToString()
        {
            return
                $"[{nameof(DirectoryEntry)}]\n" +
                $"{nameof(EntryFlags)}: {EntryFlags}\n" +
                $"{nameof(TextureFlags)}: {TextureFlags}\n" +
                $"{nameof(Offset)}: {Offset}\n" +
                $"{nameof(CompressedSize)}: {CompressedSize}\n" +
                $"{nameof(UncompressedSize)}: {UncompressedSize}";
        }
    }

    /// <summary>
    /// No clue, but most entries have this as '257'. In Titanfall 2, .bsp and .bsp_lump has this at one, as well as some random .txt files.
    /// </summary>
    [Flags]
    public enum EntryFlags : uint
    {
        None,
        /// <summary>
        /// Set on pretty much every file along with <see cref="Unknown2"/>.
        /// </summary>
        Unknown1 = 1 << 0,  // 0x00000001 - 1
        /// <summary>
        /// Set on pretty much every file along with <see cref="Unknown1"/>.
        /// </summary>
        Unknown2 = 1 << 8,  // 0x00000100 - 256
        /// <summary>
        /// Set on some .vtfs along with the previous flags and <see cref="Unknown4"/>.
        /// </summary>
        Unknown3 = 1 << 18, // 0x00040000 - 262144
        /// <summary>
        /// Set on some .vtfs along with the previous flags and <see cref="Unknown3"/>.
        /// </summary>
        Unknown4 = 1 << 19, // 0x00080000 - 524288
        /// <summary>
        /// Set on quite a few .vtfs, along with <see cref="Unknown1"/> and <see cref="Unknown2"/>.
        /// </summary>
        Unknown5 = 1 << 20  // 0x00100000 - 1048576
    }

    /// <summary>
    /// Unknown texture-related flags. *Usually* non-zero when the data this entry points to is a texture.
    /// </summary>
    [Flags]
    public enum EntryTextureFlags : ushort
    {
        /// <summary>
        /// None. Used for every entry that isn't a texture, and on a few that are.
        /// </summary>
        None,
        /// <summary>
        /// Unknown. Used for the majority of .vtfs, but a comparison in VTFEdit shows no commonalities between extracted textures with this flag set. Perhaps determines how the texture is used or loaded in-game?
        /// </summary>
        Unknown1 = 1 << 3, // 0x8
        /// <summary>
        /// Unknown. Seen in Titanfall 2 only; flag is set when this entry points to a default cubemap at "<c>materials\engine\defaultcubemap.vtf</c>".
        /// </summary>
        Unknown2 = 1 << 10 // 0x400
    }
}