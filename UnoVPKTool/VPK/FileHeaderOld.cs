using System.Runtime.InteropServices;

namespace UnoVPKTool.VPK
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = FileHeaderSize)]
    public struct FileHeaderOld
    {
        public const int FileHeaderSize = 16;

        // Some of these are wrong - examine later
        // The .vpk dir file has room for four four-byte values, including the signature and version (if it is the version, anyway... probably not).
        /// <summary>
        /// The signature (magic number) for valid .vpk files.
        /// </summary>
        public const uint Magic = 0x55AA1234;

        // Possibly not used or turned into ushort? ushort matches v2, but that means there's an extra ushort after it with the value of 3... subversion?
        //public const uint SupportedVersion = 196610;
        public const ushort SupportedVersionMajor = 2;
        public const ushort SupportedVersionMinor = 3;

        [FieldOffset(0)]
        private readonly uint _signature;

        [FieldOffset(4)]
        private readonly ushort _versionMajor;

        [FieldOffset(6)]
        private readonly ushort _versionMinor;

        [FieldOffset(8)]
        private uint _treeSize;

        [FieldOffset(12)]
        private uint _fileDataSectionSize;

        /// <summary>
        /// The signature (magic number) of this .vpk file.
        /// </summary>
        public uint Signature => _signature;

        /// <summary>
        /// The major version of this .vpk file.
        /// </summary>
        public ushort VersionMajor => _versionMajor;

        /// <summary>
        /// The minor version of this .vpk file.
        /// </summary>
        public ushort VersionMinor => _versionMinor;

        /// <summary>
        /// The size, in bytes, of the directory tree.
        /// </summary>
        public uint TreeSize { get => _treeSize; set => _treeSize = value; }

        /// <summary>
        /// How many bytes of file content are stored in this VPK file (should be zero for all Apex Legends directory VPKs).
        /// </summary>
        public uint FileDataSectionSize { get => _fileDataSectionSize; set => _fileDataSectionSize = value; }

        public override string ToString()
        {
            return
                $"Signature: 0x{Signature:X}\n" +
                $"Version: {VersionMajor}.{VersionMinor}\n" +
                $"Tree Size: {TreeSize}\n" +
                $"File Data Size: {FileDataSectionSize}";
        }
    }
}