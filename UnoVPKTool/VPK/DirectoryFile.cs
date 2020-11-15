using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnoVPKTool.Exceptions;
using UnoVPKTool.Extensions;

namespace UnoVPKTool.VPK
{
    // Instance Members
    /// <summary>
    /// Represents a VPK directory file.
    /// </summary>
    public partial class DirectoryFile : IBinaryWritable
    {
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
        /// The absolute path to this VPK directory file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// The entry blocks contained within this file.
        /// </summary>
        public IList<DirectoryEntryBlock> EntryBlocks { get; }

        /// <summary>
        /// The VPK archives that entries within this <see cref="DirectoryFile"/> point to. The indices within this are equal to a block's <see cref="DirectoryEntryBlock.ArchiveIndex"/>.
        /// </summary>
        public string[] Archives { get; }

        /// <summary>
        /// Creates a new <see cref="DirectoryFile"/> from the given path to a VPK directory file.
        /// </summary>
        /// <param name="path">The path to a VPK directory file.</param>
        /// <param name="vpkArchiveDirectory">This is the location that is searched for matching VPK archive files. If empty, uses the path this VPK directory file is in.</param>
        /// <exception cref="InvalidDataException"/>
        public DirectoryFile(string path, string vpkArchiveDirectory = "")
        {
            if (!Path.IsPathFullyQualified(path)) path = Path.GetFullPath(path);
            FilePath = path;

            using var fileStream = File.OpenRead(path);
            using var reader = new BinaryReader(fileStream);

            Magic = reader.ReadUInt32();
            if (Magic != ExpectedMagic) throw new InvalidVPKFileException($"Incorrect magic: got '0x{Magic:X4}', expected '0x{ExpectedMagic:X4}'");

            var versionMajor = reader.ReadUInt16();
            var versionMinor = reader.ReadUInt16();
            Version = new Version(versionMajor, versionMinor);
            if (!VersionIsSupported(Version)) throw new UnsupportedVPKFileException($"Unsupported version: got '{Version}', expected one of '[{string.Join<Version>(", ", SupportedVersions)}]'");

            TreeSize = reader.ReadUInt32();
            FileDataSectionSize = reader.ReadUInt32();

            EntryBlocks = new List<DirectoryEntryBlock>();
            ushort maxArchiveIndex = 0;
            var blocks = reader.ReadEntryBlocks();
            foreach (var block in blocks)
            {
                EntryBlocks.Add(block);
                if (block.ArchiveIndex > maxArchiveIndex) maxArchiveIndex = block.ArchiveIndex;
            }

            Archives = new string[maxArchiveIndex + 1];
            for (ushort i = 0; i < maxArchiveIndex + 1; i++)
            {
                string archivePath = Utils.DirectoryPathToArchivePath(FilePath, i, vpkArchiveDirectory);
                Archives[i] = archivePath;
            }
        }

        /// <summary>
        /// Reads data pointed to by the given block from the appropriate archive and then decompresses it before returning it.
        /// </summary>
        /// <param name="block"></param>
        /// <returns>A decompressed array of bytes.</returns>
        public byte[] ExtractBlock(DirectoryEntryBlock block)
        {
            return Extractor.ExtractBlock(FilePath, block, Archives[block.ArchiveIndex]);
        }

        public void Write(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return
                $"[{nameof(DirectoryFile)}]\n" +
                $"{nameof(FilePath)}: {FilePath}\n" +
                $"{nameof(Magic)}: 0x{Magic:X4}\n" +
                $"{nameof(Version)}: {Version}\n" +
                $"{nameof(TreeSize)}: {TreeSize}\n" +
                $"{nameof(FileDataSectionSize)}: {FileDataSectionSize}";
        }
    }

    // Static Members
    public partial class DirectoryFile
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