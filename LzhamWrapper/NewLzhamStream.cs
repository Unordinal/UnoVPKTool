using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LzhamWrapper.Compression;
using LzhamWrapper.Decompression;
using LzhamWrapper.Exceptions;

namespace LzhamWrapper
{
    public class NewLzhamStream : Stream
    {
        private const string MsgHandleClosed = "The handle to the {0} was closed or invalid.";
        private const string MsgLzhamFailed = "{0} failed: {1}";
        private const string MsgInitNotSupported = "The stream cannot be initialized for {0} as the base stream does not support {1}.";
        private const string MsgInvalidParams = "Could not initialize {0} with the specified parameters.";
        private const string MsgNotSupported = "This operation is not supported on this stream.";
        private const string MsgStreamDisposed = "The stream is closed.";
        private const int DefaultBufferSize = 8192;

        private Stream _stream;
        private Compressor? _compressor;
        private Decompressor? _decompressor;
        private byte[]? _buffer;
        private bool _leaveOpen;
        private bool _activeAsyncOperation;
        private bool _wroteBytes;

        public override bool CanRead => _stream?.CanRead == true && _decompressor is not null;

        public override bool CanWrite => _stream?.CanWrite == true && _compressor is not null;

        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException(MsgNotSupported);

        public override long Position
        {
            get => throw new NotSupportedException(MsgNotSupported);
            set => throw new NotSupportedException(MsgNotSupported);
        }

        /// <summary>
        /// Initializes a new <see cref="NewLzhamStream"/> in compression mode.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="compressionParameters"></param>
        /// <param name="leaveOpen"></param>
        public NewLzhamStream(Stream stream, CompressionParameters compressionParameters, bool leaveOpen = false)
            : this(stream, compressionParameters, null, leaveOpen) { }

        /// <summary>
        /// Initializes a new <see cref="NewLzhamStream"/> in decompression mode.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="decompressionParameters"></param>
        /// <param name="leaveOpen"></param>
        public NewLzhamStream(Stream stream, DecompressionParameters decompressionParameters, bool leaveOpen = false)
            : this(stream, null, decompressionParameters, leaveOpen) { }

        /// <summary>
        /// Initializes a new <see cref="NewLzhamStream"/> with the capability to both compress and decompress.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="compressionParameters"></param>
        /// <param name="decompressionParameters"></param>
        /// <param name="leaveOpen"></param>
        public NewLzhamStream(Stream stream, CompressionParameters compressionParameters, DecompressionParameters decompressionParameters, bool leaveOpen = false)
            : this(stream, (CompressionParameters?)compressionParameters, (DecompressionParameters?)decompressionParameters, leaveOpen) { }

        private NewLzhamStream(Stream stream, CompressionParameters? compressionParameters, DecompressionParameters? decompressionParameters, bool leaveOpen)
        {
            Debug.Assert(compressionParameters is not null || decompressionParameters is not null);

            if (stream is null) throw new ArgumentNullException(nameof(stream));

            if (compressionParameters is not null)
            {
                if (!stream.CanWrite)
                    throw new ArgumentException(string.Format(MsgInitNotSupported, "compression", "writing"), nameof(stream));

                _compressor = new Compressor(compressionParameters.Value);
            }
            if (decompressionParameters is not null)
            {
                if (!stream.CanRead)
                    throw new ArgumentException(string.Format(MsgInitNotSupported, "decompression", "reading"), nameof(stream));

                _decompressor = new Decompressor(decompressionParameters.Value);
            }

            _stream = stream;
            _leaveOpen = leaveOpen;
            InitializeBuffer();
        }

        public override int Read(Span<byte> buffer)
        {
            return InternalRead(buffer, buffer.Length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Utils.ValidateArrayBounds(buffer, offset, count);

            return Read(buffer.AsSpan(offset, count));
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            InternalWrite(buffer);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Utils.ValidateArrayBounds(buffer, offset, count);
            Write(buffer.AsSpan(offset, count));
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This operation is not supported on this stream and will always throw a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(MsgNotSupported);
        }

        /// <summary>
        /// This operation is not supported on this stream and will always throw a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        private int InternalRead(Span<byte> buffer, int compressedBytesCount)
        {
            EnsureDecompressionMode();
            EnsureNotDisposed();
            EnsureBufferInitialized();

            Debug.Assert(_decompressor is not null);

            DecompressStatus status = DecompressStatus.Failure;
            int totalRead = 0;
            int totalToRead = compressedBytesCount;
            int totalWritten = 0;
            int bytesFromStream = 0;
            bool noMoreInputBytes = false;
            bool initial = true;
            do
            {
                do
                {
                    // If this is the inital loop, we wanna try and get bytes from the stream first.
                    if (initial) break;

                    // Limit the number of bytes we read to the internal buffer size.
                    int maxToRead = Math.Min(totalToRead, _buffer.Length);
                    noMoreInputBytes = (totalRead + maxToRead >= totalToRead) || bytesFromStream == 0;
                    ReadOnlySpan<byte> input = _buffer.AsSpan(0, maxToRead);

                    status = _decompressor.Decompress(input, out int bytesRead, buffer[totalRead..], out int bytesWritten, noMoreInputBytes);
                    totalRead += bytesRead;
                    totalWritten += bytesWritten;
                }
                // While the decompressor has more output and we've not read in all of the requested bytes.
                while (status == DecompressStatus.HasMoreOutput && totalRead < totalToRead);

                // If we don't have any bytes, we'll try to read some into the input buffer. If we can't, we're done.
                if (bytesFromStream == 0)
                {
                    initial = false;
                    bytesFromStream = _stream.Read(_buffer); // Try to read in 'DefaultBufferSize' amount of bytes.
                    if (bytesFromStream > _buffer.Length)
                        throw new InvalidDataException("Too many bytes were returned by the stream.");
                }
            }
            // While the decompressor hasn't finished, we've not read in all of the requested bytes and there are more bytes to read.
            while (status < DecompressStatus.FirstSuccessOrFailureCode && totalRead < totalToRead && bytesFromStream > 0);

            // If we're here and the status isn't Success, something went wrong.
            if (status != DecompressStatus.Success)
                throw new LzhamException(string.Format(MsgLzhamFailed, "Decompression", status));

            return totalWritten;
        }

        private void InternalWrite(ReadOnlySpan<byte> buffer)
        {

        }

        [MemberNotNull(nameof(_buffer))]
        private void InitializeBuffer()
        {
            Debug.Assert(_buffer is null);
            _buffer = ArrayPool<byte>.Shared.Rent(DefaultBufferSize);
        }

        [MemberNotNull(nameof(_stream))]
        private void EnsureNotDisposed()
        {
            if (_stream is null)
                throw new ObjectDisposedException(null, MsgStreamDisposed);
        }

        [MemberNotNull(nameof(_buffer))]
        private void EnsureBufferInitialized()
        {
            if (_buffer is null)
                InitializeBuffer();
        }

        [MemberNotNull(nameof(_compressor))]
        private void EnsureCompressionMode()
        {
            if (_compressor is null)
                throw new InvalidOperationException(MsgNotSupported);
        }
        
        [MemberNotNull(nameof(_decompressor))]
        private void EnsureDecompressionMode()
        {
            if (_decompressor is null)
                throw new InvalidOperationException(MsgNotSupported);
        }
    }
}