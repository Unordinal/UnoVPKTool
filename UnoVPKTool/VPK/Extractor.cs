using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LzhamWrapper;
using LzhamWrapper.Decompression;

namespace UnoVPKTool.VPK
{
    // Instance Members
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
    public sealed partial class Extractor : IDisposable
    {
        private readonly FileStream[] _archiveStreams;
        private readonly DirectoryFile _file;
        private readonly Decompressor _decompressor;

        public Extractor(DirectoryFile file, bool useAsync = false)
        {
            _archiveStreams = new FileStream[file.Archives.Length];
            for (int i = 0; i < _archiveStreams.Length; i++)
            {
                _archiveStreams[i] = new FileStream(file.Archives[i], FileMode.Open, FileAccess.Read, FileShare.Read, 8196, useAsync);
            }

            _file = file;
            var parameters = new DecompressionParameters { DictionarySize = Lzham.ApexDictSize, Flags = DecompressionFlags.OutputUnbuffered };
            _decompressor = new Decompressor(parameters);
        }

        public void Dispose()
        {
            for (int i = 0; i < _archiveStreams.Length; i++)
            {
                _archiveStreams[i].Dispose();
            }
            _decompressor.Dispose();
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

        public void ExtractAllStream(string basePath)
        {
            var lzhamStreams = new List<LzhamStream>();
            var decompParams = new DecompressionParameters { DictionarySize = 20, Flags = DecompressionFlags.OutputUnbuffered };
            foreach (var stream in _archiveStreams)
                lzhamStreams.Add(new LzhamStream(stream, decompParams, true));

            var decompBlocks = ReadAndDecompressRawBlocks(lzhamStreams.ToArray(), _file.EntryBlocks);
            foreach (var (rawBlock, block) in decompBlocks)
            {
                WriteRawBlock(basePath, rawBlock, block);
            }

            foreach (var stream in lzhamStreams)
                stream.Close();
        }

        /// <summary>
        /// Asynchronously extracts all blocks in the given file to the specified base directory.
        /// </summary>
        /// <param name="basePath"></param>
        public async Task ExtractAllAsync(string basePath, IProgress<EntryOperation>? progress = null, CancellationToken cancellationToken = default)
        {
            var decompBlocks = DecompressRawBlocksAsync(ReadRawBlocksAsync(_archiveStreams, _file.EntryBlocks, progress, cancellationToken), progress, cancellationToken);
            await foreach (var (rawBlock, block) in decompBlocks.WithCancellation(cancellationToken))
            {
                await WriteRawBlockAsync(basePath, rawBlock, block, cancellationToken);
            }
        }

        /// <summary>
        /// Extracts a specific file from the VPK.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="fileName"></param>
        public void Extract(string basePath, string fileName)
        {
            Extract(basePath, fileName);
        }

        /// <summary>
        /// Extracts many files from the VPK.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="fileNames"></param>
        public void Extract(string basePath, params string[] fileNames)
        {
            var targets = _file.EntryBlocks.Where(b => fileNames.Contains(Path.GetFileName(b.FilePath)));
            var decompBlocks = DecompressRawBlocks(ReadRawBlocks(_archiveStreams, targets));
            WriteRawBlocks(basePath, decompBlocks);
        }
    }

    public sealed partial class Extractor
    {
        public static IEnumerable<(byte[], DirectoryEntryBlock)> ReadAndDecompressRawBlocks(LzhamStream[] archiveStreams, IEnumerable<DirectoryEntryBlock> blocks)
        {
            foreach (var block in blocks)
            {
                byte[] decmpBlock = new byte[block.TotalUncompressedSize];
                ReadAndDecompressRawBlock(archiveStreams[block.ArchiveIndex], decmpBlock, block);
                yield return (decmpBlock, block);
            }
        }

        public static void ReadAndDecompressRawBlock(LzhamStream archiveStream, Span<byte> buffer, DirectoryEntryBlock block)
        {
            int offset = 0;
            foreach (var entry in block.Entries)
            {
                var entrySlice = buffer.Slice(offset, (int)entry.UncompressedSize);
                ReadAndDecompressRawEntry(archiveStream, entrySlice, entry);
                offset += (int)entry.UncompressedSize;
            }
        }

        public static void ReadAndDecompressRawEntry(LzhamStream archiveStream, Span<byte> buffer, DirectoryEntry entry)
        {
            archiveStream.Seek((long)entry.Offset, SeekOrigin.Begin);
            if (entry.IsCompressed)
                archiveStream.Read(buffer, (int)entry.CompressedSize);
            else
                archiveStream.BaseStream.Read(buffer);
        }
    }

    // Static Members - Synchronous
    public sealed partial class Extractor
    {
        #region Reading

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
        /// Reads a raw block from the stream into the given buffer.
        /// </summary>
        /// <param name="archiveStreams"></param>
        /// <param name="buffer"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public static void ReadRawBlock(Stream archiveStream, Span<byte> buffer, DirectoryEntryBlock block)
        {
            int offset = 0;
            foreach (var entry in block.Entries)
            {
                var entrySlice = buffer.Slice(offset, (int)entry.UncompressedSize);
                ReadRawEntry(archiveStream, entrySlice, entry.Offset);
                offset += (int)entry.UncompressedSize;
            }
        }

        /// <summary>
        /// Reads a raw entry from the stream at the specified offset into the given buffer.
        /// </summary>
        /// <param name="archiveStream"></param>
        /// <param name="buffer"></param>
        /// <param name="entryOffset"></param>
        /// <returns></returns>
        public static void ReadRawEntry(Stream archiveStream, Span<byte> buffer, ulong entryOffset)
        {
            archiveStream.Seek((long)entryOffset, SeekOrigin.Begin);
            archiveStream.Read(buffer);
        }

        #endregion Reading

        #region Writing

        /// <summary>
        /// Writes a collection of raw blocks of data to a file in the specified <paramref name="basePath"/>.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="rawBlocks"></param>
        /// <param name="blocks"></param>
        public static void WriteRawBlocks(string basePath, byte[][] rawBlocks, DirectoryEntryBlock[] blocks)
        {
            if (rawBlocks.Length != blocks.Length) throw new ArgumentException("Block list length did not match raw block list length!");

            for (int i = 0; i < rawBlocks.Length; i++)
            {
                WriteRawBlock(basePath, rawBlocks[i], blocks[i]);
            }
        }

        /// <summary>
        /// Writes a collection of raw blocks of data to a file in the specified <paramref name="basePath"/>.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="rawBlocks"></param>
        /// <param name="blocks"></param>
        public static void WriteRawBlocks(string basePath, IEnumerable<(byte[], DirectoryEntryBlock)> blocks)
        {
            foreach (var (rawBlock, block) in blocks)
            {
                WriteRawBlock(basePath, rawBlock, block);
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

        #endregion Writing

        #region Decompression

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
        /// Decompresses a raw entry in the buffer of length <see cref="DirectoryEntry.UncompressedSize"/> into the same buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="compressedSize"></param>
        public static void DecompressRawEntry(Span<byte> buffer, int compressedSize)
        {
            var entrySlice = buffer.Slice(0, compressedSize);
            Lzham.DecompressMemory(entrySlice.ToArray(), (ulong)buffer.Length).CopyTo(buffer);
        }

        #endregion Decompression
    }

    // Static Members - Asynchronous
    public partial class Extractor
    {
        #region Reading

        /// <summary>
        /// Asynchronously reads the given blocks using the appropriate passed-in streams. Indices must match that of the initial file's <see cref="DirectoryFile.Archives"/>.
        /// </summary>
        /// <param name="archiveStreams"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<(byte[], DirectoryEntryBlock)> ReadRawBlocksAsync(
            Stream[] archiveStreams,
            IEnumerable<DirectoryEntryBlock> blocks,
            IProgress<EntryOperation>? progress = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var block in blocks)
            {
                byte[] buffer = new byte[block.TotalUncompressedSize];
                await ReadRawBlockAsync(archiveStreams[block.ArchiveIndex], buffer, block, progress, cancellationToken);
                yield return (buffer, block);
            }
        }

        /// <summary>
        /// Asynchronously reads a raw block from the stream into the given buffer.
        /// </summary>
        /// <param name="archiveStreams"></param>
        /// <param name="buffer"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public static async Task ReadRawBlockAsync(
            Stream archiveStream,
            Memory<byte> buffer,
            DirectoryEntryBlock block,
            IProgress<EntryOperation>? progress = null,
            CancellationToken cancellationToken = default)
        {
            int offset = 0;
            foreach (var entry in block.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entrySlice = buffer.Slice(offset, (int)entry.UncompressedSize);
                await ReadRawEntryAsync(archiveStream, entrySlice, entry.Offset, cancellationToken);
                offset += (int)entry.UncompressedSize;

                progress?.Report(EntryOperation.Read);
            }
        }

        /// <summary>
        /// Asynchronously reads a raw entry from the stream at the specified offset into the given buffer.
        /// </summary>
        /// <param name="archiveStream"></param>
        /// <param name="buffer"></param>
        /// <param name="entryOffset"></param>
        /// <returns></returns>
        public static async ValueTask ReadRawEntryAsync(
            Stream archiveStream,
            Memory<byte> buffer,
            ulong entryOffset,
            CancellationToken cancellationToken = default)
        {
            archiveStream.Seek((long)entryOffset, SeekOrigin.Begin);
            await archiveStream.ReadAsync(buffer, cancellationToken);
        }

        #endregion Reading

        #region Writing

        /// <summary>
        /// Asynchronously writes a raw block of data to a file, combining <paramref name="basePath"/> with block's <see cref="DirectoryEntryBlock.FilePath"/>. Directories will be created as necessary.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="rawBlock"></param>
        /// <param name="block"></param>
        public static async Task WriteRawBlockAsync(
            string basePath,
            byte[] rawBlock,
            DirectoryEntryBlock block,
            CancellationToken cancellationToken = default)
        {
            string path = Path.Combine(basePath, block.FilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, (int)block.TotalUncompressedSize, true);
            await fs.WriteAsync(rawBlock, cancellationToken);
        }

        #endregion Writing

        #region Decompression

        /// <summary>
        /// Asynchronously decompresses the given blocks.
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<(byte[], DirectoryEntryBlock)> DecompressRawBlocksAsync(
            IAsyncEnumerable<(byte[], DirectoryEntryBlock)> blocks,
            IProgress<EntryOperation>? progress = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var (rawBlock, block) in blocks.WithCancellation(cancellationToken))
            {
                await DecompressRawBlockAsync(rawBlock, block, progress, cancellationToken);
                yield return (rawBlock, block);
            }
        }

        /// <summary>
        /// Asynchronously decompresses the raw blocks in the given buffer of length <see cref="DirectoryEntryBlock.TotalUncompressedSize"/> into the same buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="block"></param>
        public static async Task DecompressRawBlockAsync(
            Memory<byte> buffer,
            DirectoryEntryBlock block,
            IProgress<EntryOperation>? progress = null,
            CancellationToken cancellationToken = default)
        {
            int offset = 0;
            foreach (var entry in block.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entrySlice = buffer.Slice(offset, (int)entry.UncompressedSize);
                if (entry.IsCompressed)
                {
                    await DecompressRawEntryAsync(entrySlice, (int)entry.CompressedSize, cancellationToken);
                }
                offset += (int)entry.UncompressedSize;

                progress?.Report(EntryOperation.Decompress);
            }
        }

        /// <summary>
        /// Asynchronously decompresses a raw entry in the buffer of length <see cref="DirectoryEntry.UncompressedSize"/> into the same buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="compressedSize"></param>
        public static async Task DecompressRawEntryAsync(
            Memory<byte> buffer,
            int compressedSize,
            CancellationToken cancellationToken = default)
        {
            var entrySlice = buffer.Slice(0, compressedSize);
            await Task.Run(() => Lzham.DecompressMemory(entrySlice.ToArray(), (ulong)buffer.Length).CopyTo(buffer), cancellationToken);
        }

        #endregion Decompression
    }

    public readonly struct EntryOperation
    {
        public enum ProcessType { Unknown, Read, Write, Compress, Decompress }

        public static readonly EntryOperation Read = new EntryOperation(ProcessType.Read);
        public static readonly EntryOperation Write = new EntryOperation(ProcessType.Write);
        public static readonly EntryOperation Compress = new EntryOperation(ProcessType.Compress);
        public static readonly EntryOperation Decompress = new EntryOperation(ProcessType.Decompress);

        public ProcessType OperationPerformed { get; init; }

        public EntryOperation(ProcessType processType)
        {
            OperationPerformed = processType;
        }
    }
}