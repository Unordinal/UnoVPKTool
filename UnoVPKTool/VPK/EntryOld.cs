using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace UnoVPKTool.VPK
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = ConstSize)]
    public struct EntryOld
    {
        public const int ConstSize = 40;
        public const ushort Terminator = ushort.MaxValue;

        [FieldOffset(0)]
        private uint _crc;

        [FieldOffset(4)]
        private ushort _preloadBytes; // Always 0?

        [FieldOffset(6)]
        private ushort _archiveIndex; // The "_00#.vpk" archive file the data is in.

        [FieldOffset(8)]
        public uint _unknown1; // Usually 257, sometimes 1 (possible Entry->data->ID?)
        
        [FieldOffset(12)]
        public ushort _unknown2; // Usually 0, sometimes 16, (sometimes >524000 (no longer?))

        [FieldOffset(14)]
        private uint _offset;
        
        [FieldOffset(18)]
        public uint _unknown3; // Always 0?

        [FieldOffset(22)]
        private uint _length; // Compressed data size.
        
        [FieldOffset(26)]
        public uint _unknown4; // Always 0?

        [FieldOffset(30)]
        private uint _fileSize; // Uncompressed file size.

        [FieldOffset(34)]
        public uint _unknown5; // Always 0?

        /// <summary>
        /// A 32-bit CRC of the specified file's data.
        /// </summary>
        public uint CRC { get => _crc; set => _crc = value; }

        /// <summary>
        /// The number of bytes contained in the index (directory) file.
        /// </summary>
        public ushort PreloadBytes { get => _preloadBytes; set => _preloadBytes = value; }

        /// <summary>
        /// A zero-based index of the archive this file's data is contained in. If 0x7FFF, the data follows the directory.
        /// </summary>
        public ushort ArchiveIndex { get => _archiveIndex; set => _archiveIndex = value; }

        /// <summary>
        /// The offset of the data from the start of the specified archive. If <see cref="ArchiveIndex"/> is <c>0x7FFF</c>, this is the offset of the file data relative to the end of the directory (see <see cref="FileHeaderOld"/> for more details).
        /// </summary>
        public uint Offset { get => _offset; set => _offset = value; }

        /// <summary>
        /// The number of bytes stored starting at <see cref="Offset"/>. If zero, the entire file is stored in the preload data.
        /// </summary>
        public uint Length { get => _length; set => _length = value; }

        /// <summary>
        /// The size of the specified archive, in bytes. Equal to <c><see cref="PreloadBytes"/> + <see cref="Length"/></c>.
        /// </summary>
        public uint ArchiveSize => PreloadBytes + Length;

        public byte[] ToBytes()
        {
            return new byte[0];
        }

        public override string ToString()
        {
            //return Utils.DumpInstanceFields(this);
            return
                $"{nameof(CRC)}: {CRC}\n" +
                $"{nameof(PreloadBytes)}: {PreloadBytes}\n" +
                $"{nameof(ArchiveIndex)}: {ArchiveIndex}\n" +
                $"{nameof(Offset)}: {Offset}\n" +
                $"{nameof(Length)}: {Length}\n" +
                $"{nameof(ArchiveSize)}: {ArchiveSize}";
        }
    }
}