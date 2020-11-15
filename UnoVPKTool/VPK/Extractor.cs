using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace UnoVPKTool.VPK
{
    public sealed class Extractor : IDisposable
    {
        private MemoryMappedFile[] _archiveMappedFiles;
        private MemoryMappedViewStream[] _archiveStreams;
        private DirectoryFile _file;

        public Extractor(DirectoryFile file)
        {
            int archiveCount = file.Archives.Length;
            _archiveMappedFiles = new MemoryMappedFile[archiveCount];
            _archiveStreams = new MemoryMappedViewStream[archiveCount];
            for (int i = 0; i < _archiveMappedFiles.Length; i++)
            {
                var mmf = MemoryMappedFile.CreateFromFile(file.Archives[i]);
                var mmvs = mmf.CreateViewStream();
                _archiveMappedFiles[i] = mmf;
                _archiveStreams[i] = mmvs;
            }

            _file = file;
        }

        public void Dispose()
        {
            for (int i = 0; i < _archiveMappedFiles.Length; i++)
            {
                _archiveStreams[i].Dispose();
                _archiveMappedFiles[i].Dispose();
            }
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
        private static IEnumerable<(byte[], DirectoryEntryBlock)> ReadRawBlocks(Stream[] archiveStreams, IEnumerable<DirectoryEntryBlock> blocks)
        {
            foreach (var block in blocks)
            {
                byte[] buffer = new byte[block.TotalUncompressedSize];
                ReadRawBlock(archiveStreams, buffer, block);
                yield return (buffer, block);
            }
        }

        /// <summary>
        /// Decompresses the given blocks.
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns></returns>
        private static IEnumerable<(byte[], DirectoryEntryBlock)> DecompressRawBlocks(IEnumerable<(byte[], DirectoryEntryBlock)> blocks)
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
        private static int ReadRawBlock(Stream[] archiveStreams, Span<byte> buffer, DirectoryEntryBlock block)
        {
            int offset = 0;
            foreach (var entry in block.Entries)
            {
                var entrySlice = buffer.Slice(offset, (int)entry.UncompressedSize);
                offset += ReadRawEntry(archiveStreams[block.ArchiveIndex], entrySlice, entry.Offset);
            }
            return offset;
        }

        /// <summary>
        /// Decompresses the raw blocks in the given buffer of length <see cref="DirectoryEntryBlock.TotalUncompressedSize"/> into the same buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="block"></param>
        private static void DecompressRawBlock(Span<byte> buffer, DirectoryEntryBlock block)
        {
            int offset = 0;
            foreach (var entry in block.Entries)
            {
                var entrySlice = buffer.Slice(offset, (int)entry.UncompressedSize);
                if (entry.IsCompressed)
                {
                    DecompressRawEntry(entrySlice, entry.CompressedSize);
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
        private static void WriteRawBlock(string basePath, byte[] rawBlock, DirectoryEntryBlock block)
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
        private static int ReadRawEntry(Stream archiveStream, Span<byte> buffer, ulong entryOffset)
        {
            archiveStream.Seek((long)entryOffset, SeekOrigin.Begin);
            return archiveStream.Read(buffer);
        }

        /// <summary>
        /// Decompresses a raw entry in the buffer of length <see cref="DirectoryEntry.UncompressedSize"/> into the same buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="compressedSize"></param>
        private static void DecompressRawEntry(Span<byte> buffer, ulong compressedSize)
        {
            var entrySlice = buffer.Slice(0, (int)compressedSize);
            Lzham.DecompressMemory(entrySlice.ToArray(), (ulong)buffer.Length).CopyTo(buffer);
        }
    }
}