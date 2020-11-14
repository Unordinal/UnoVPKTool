using System;
using System.Collections.Generic;
using System.IO;
using UnoVPKTool.Exceptions;
using UnoVPKTool.Extensions;

namespace UnoVPKTool.VPK
{
    /// <summary>
    /// Represents a VPK directory file.
    /// </summary>
    public class DirectoryFile : IBinaryWritable
    {
        /// <summary>
        /// The signature (magic number) for VPK directory files.
        /// </summary>
        public const uint ExpectedMagic = 0x55AA1234;
        /// <summary>
        /// The supported VPK versions. Currently contains only v2.3.
        /// </summary>
        public static readonly Version[] SupportedVersions = { new Version(2, 3) };

        /// <summary>
        /// The signature of this VPK file. Should equal <see cref="ExpectedMagic"/>.
        /// </summary>
        public uint Magic { get; init; }

        /// <summary>
        /// The version of this VPK file.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The size of the VPK directory tree, in bytes.
        /// </summary>
        public uint TreeSize { get; set; }

        /// <summary>
        /// The number of bytes of file content that are stored in this VPK directory file.
        /// <br/>
        /// Should be zero for all Apex Legends VPK directory files.
        /// </summary>
        public uint FileDataSectionSize { get; set; }

        /// <summary>
        /// The directory tree of the VPK file. It is three levels deep, starting at the root node within the tree:
        /// <list type="number">
        ///     <item>Extensions.</item>
        ///     <item>Paths.</item>
        ///     <item>File names.</item>
        /// </list>
        /// </summary>
        public Tree Tree { get; }

        /// <summary>
        /// The entry blocks contained within this file.
        /// </summary>
        public IList<DirectoryEntryBlock> EntryBlocks { get; }

        /// <summary>
        /// Initializes a new VPK file from the given <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="InvalidDataException"/>
        public DirectoryFile(BinaryReader reader)
        {
            Magic = reader.ReadUInt32();
            if (Magic != ExpectedMagic) throw new InvalidVPKFileException($"Incorrect magic: got '0x{Magic:X4}', expected '0x{ExpectedMagic:X4}'");

            var versionMajor = reader.ReadUInt16();
            var versionMinor = reader.ReadUInt16();
            Version = new Version(versionMajor, versionMinor);
            if (!VersionIsSupported(Version)) throw new UnsupportedVPKFileException($"Unsupported version: got '{Version}', expected one of '[{string.Join<Version>(", ", SupportedVersions)}]'");

            TreeSize = reader.ReadUInt32();
            FileDataSectionSize = reader.ReadUInt32();
            Tree = reader.ReadTree(out var entryBlocks);
            EntryBlocks = entryBlocks;
        }

        // Untested, still unimplemented for Tree.
        public void Write(BinaryWriter writer)
        {
            throw new NotImplementedException();
            writer.Write(Magic);
            writer.Write((ushort)Version.Major);
            writer.Write((ushort)Version.Minor);

            byte[]? treeBytes = null;
            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter treeWriter = new BinaryWriter(memStream))
                {
                    Tree.Write(treeWriter);
                    treeBytes = memStream.ToArray();
                    TreeSize = (uint)treeBytes.Length;

                    writer.Write(TreeSize);
                }
            }

            uint fileData = 0;
            Tree.RootNode.Traverse((n) =>
            {
                if (n.EntryBlock is not null) fileData += n.EntryBlock.PreloadBytes;
            });
            FileDataSectionSize = fileData;

            writer.Write(FileDataSectionSize);
            writer.Write(treeBytes);
        }

        /// <summary>
        /// Retrieves data in an archive from the specified offset.
        /// </summary>
        /// <param name="offset">The offset in the archive.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns></returns>
        /*public byte[] GetDataAtOffset(ulong offset, ulong count)
        {

        }*/

        public override string ToString()
        {
            return
                $"{nameof(Magic)}: 0x{Magic:X4}\n" +
                $"{nameof(Version)}: {Version}\n" +
                $"{nameof(TreeSize)}: {TreeSize}\n" +
                $"{nameof(FileDataSectionSize)}: {FileDataSectionSize}";
        }
        
        /// <summary>
        /// Checks if the given VPK version is supported.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool VersionIsSupported(Version version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            foreach (var supported in SupportedVersions)
            {
                if (supported.Major == version.Major && supported.Minor == version.Minor) return true;
            }

            return false;
        }
    }
}