using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace UnoVPKTool.VPK
{
    /// <summary>
    /// A class that helps with extracting content from VPK archives.
    /// </summary>
    /// <remarks>
    /// Notes: 
    /// <br/>
    /// Whenever the words 'block' or 'entry block' are used, they are referring to a single file whose piece(s) reside in a VPK archive file ('<c>pak000_###</c>' where <c>###</c> denotes the archive index).
    /// <br/>
    /// An entry block can be one piece, or split up into multiple pieces. Each piece is referred to as an 'entry'.
    /// <br/>
    /// An entry is simply a length of bytes stored at an offset in a VPK archive, and is usually compressed using LZHAM -- specifically, LZHAM alpha8 with a dict size of 20.
    /// </remarks>
    public sealed class Extractor : IDisposable
    {
        private readonly FileStream[] _archiveStreams;
        private readonly DirectoryFile _file;

        public Extractor(DirectoryFile file)
        {
            _archiveStreams = new FileStream[file.Archives.Length];
            for (int i = 0; i < _archiveStreams.Length; i++)
            {
                _archiveStreams[i] = new FileStream(file.Archives[i], FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.None);
            }

            _file = file;
        }

        public void Dispose()
        {
            for (int i = 0; i < _archiveStreams.Length; i++)
            {
                _archiveStreams[i].Dispose();
            }
        }

        /// <summary>
        /// Returns an appropriate stream for the specified archive index.
        /// </summary>
        /// <param name="archiveIndex"></param>
        /// <returns></returns>
        public Stream GetStreamForArchive(ushort archiveIndex)
        {
            return _archiveStreams[archiveIndex];
        }

        /// <summary>
        /// Extracts all blocks in the given file to the specified base directory.
        /// </summary>
        /// <param name="basePath"></param>
        public void ExtractAll(string basePath)
        {
            var decompBlocks = DecompressRawBlocks(ReadRawBlocks(_archiveStreams, _file.EntryBlocks));
            foreach (var (rawBlock, block) in decompBlocks)
            {
                WriteRawBlock(basePath, rawBlock, block);
            }
        }

        /// <summary>
        /// Reads the given blocks using the appropriate passed-in streams. Indices must match that of the initial file's <see cref="DirectoryFile.Archives"/>.
        /// </summary>
        /// <param name="archiveStreams"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public static IEnumerable<(byte[], DirectoryEntryBlock)> ReadRawBlocks(Stream[] archiveStreams, IEnumerable<DirectoryEntryBlock> blocks)
        {
            foreach (var block in blocks)
            {
                byte[] buffer = new byte[block.TotalUncompressedSize];
                ReadRawBlock(archiveStreams[block.ArchiveIndex], buffer, block);
                yield return (buffer, block);
            }
        }

        /// <summary>
        /// Decompresses the given blocks.
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public static IEnumerable<(byte[], DirectoryEntryBlock)> DecompressRawBlocks(IEnumerable<(byte[], DirectoryEntryBlock)> blocks)
        {
            foreach (var (rawBlock, block) in blocks)
            {
                DecompressRawBlock(rawBlock, block);
                yield return (rawBlock, block);
            }
        }

        /// <summary>
        /// Reads a raw block from the stream into the given buffer.
        /// </summary>
        /// <param name="archiveStreams"></param>
        /// <param name="buffer"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public static int ReadRawBlock(Stream archiveStream, Span<byte> buffer, DirectoryEntryBlock block)
        {
            int offset = 0;
            foreach (var entry in block.Entries)
            {
                var entrySlice = buffer.Slice(offset, (int)entry.UncompressedSize);
                ReadRawEntry(archiveStream, entrySlice, entry.Offset);
                offset += (int)entry.UncompressedSize;
            }
            return offset;
        }

        /// <summary>
        /// Decompresses the raw blocks in the given buffer of length <see cref="DirectoryEntryBlock.TotalUncompressedSize"/> into the same buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="block"></param>
        public static void DecompressRawBlock(Span<byte> buffer, DirectoryEntryBlock block)
        {
            int offset = 0;
            foreach (var entry in block.Entries)
            {
                var entrySlice = buffer.Slice(offset, (int)entry.UncompressedSize);
                if (entry.IsCompressed)
                {
                    DecompressRawEntry(entrySlice, (int)entry.CompressedSize);
                }
                offset += (int)entry.UncompressedSize;
            }
        }

        /// <summary>
        /// Writes a raw block of data to a file, combining <paramref name="basePath"/> with block's <see cref="DirectoryEntryBlock.FilePath"/>. Directories will be created as necessary.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="rawBlock"></param>
        /// <param name="block"></param>
        public static void WriteRawBlock(string basePath, byte[] rawBlock, DirectoryEntryBlock block)
        {
            string path = Path.Combine(basePath, block.FilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, (int)block.TotalUncompressedSize);
            fs.Write(rawBlock);
        }

        /// <summary>
        /// Reads a raw entry from the stream at the specified offset into the given buffer.
        /// </summary>
        /// <param name="archiveStream"></param>
        /// <param name="buffer"></param>
        /// <param name="entryOffset"></param>
        /// <returns></returns>
        public static int ReadRawEntry(Stream archiveStream, Span<byte> buffer, ulong entryOffset)
        {
            archiveStream.Seek((long)entryOffset, SeekOrigin.Begin);
            return archiveStream.Read(buffer);
        }

        /// <summary>
        /// Decompresses a raw entry in the buffer of length <see cref="DirectoryEntry.UncompressedSize"/> into the same buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="compressedSize"></param>
        public static void DecompressRawEntry(Span<byte> buffer, int compressedSize)
        {
            var entrySlice = buffer.Slice(0, compressedSize);
            Lzham.DecompressMemory(entrySlice.ToArray(), (ulong)buffer.Length).CopyTo(buffer);
        }
    }
}