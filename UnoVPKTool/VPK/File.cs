using System;
using System.IO;
using UnoVPKTool.Extensions;

namespace UnoVPKTool.VPK
{
    public class File : IBinaryWritable
    {
        /// <summary>
        /// The signature (magic number) for VPK directory files.
        /// </summary>
        public const uint ExpectedMagic = 0x55AA1234;
        public const ushort SupportedVersionMajor = 2;
        public const ushort SupportedVersionMinor = 3;

        /// <summary>
        /// The signature of this VPK file. Should equal <see cref="ExpectedMagic"/>.
        /// </summary>
        public uint Magic { get; init; }

        /// <summary>
        /// The major version of this VPK file.
        /// </summary>
        public ushort VersionMajor { get; }

        /// <summary>
        /// The minor version of this VPK file.
        /// </summary>
        public ushort VersionMinor { get; }

        /// <summary>
        /// The size of the VPK directory tree, in bytes.
        /// </summary>
        public uint TreeSize { get; set; }

        /// <summary>
        /// The numebr of bytes of file content are stored in this VPK directory file.
        /// <br/>
        /// Should be zero for all Apex Legends VPK directory files.
        /// </summary>
        public uint FileDataSectionSize { get; set; }

        public Tree Tree { get; }

        /// <summary>
        /// Initializes a new VPK file from the given <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="InvalidDataException"/>
        public File(BinaryReader reader)
        {
            Magic = reader.ReadUInt32();
            if (Magic != ExpectedMagic) throw new InvalidDataException($"Incorrect magic: got 0x{Magic:X4}, expected 0x{ExpectedMagic:X4}");

            VersionMajor = reader.ReadUInt16();
            if (VersionMajor != SupportedVersionMajor) throw new InvalidDataException($"Unsupported major version: got {VersionMajor}, expected {SupportedVersionMajor}");
            VersionMinor = reader.ReadUInt16();
            if (VersionMinor != SupportedVersionMinor) throw new InvalidDataException($"Unsupported minor version: got {VersionMinor}, expected {SupportedVersionMinor}");

            TreeSize = reader.ReadUInt32();
            FileDataSectionSize = reader.ReadUInt32();
            Tree = reader.ReadTree();
        }

        public void Write(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return
                $"{nameof(Magic)}: 0x{Magic:X4}\n" +
                $"Version: {VersionMajor}.{VersionMinor}\n" +
                $"{nameof(TreeSize)}: {TreeSize}\n" +
                $"{nameof(FileDataSectionSize)}: {FileDataSectionSize}";
        }
    }
}