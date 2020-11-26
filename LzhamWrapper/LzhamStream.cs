using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LzhamWrapper.Compression;
using LzhamWrapper.Decompression;
using LzhamWrapper.Exceptions;

namespace LzhamWrapper
{
    public class LzhamStream : Stream
    {
        private const string TemplateHandleClosedMsg = "The handle to the {0} was closed or invalid.";
        private const string TmplSingleModeMsg = "{0} is not supported on a {1} stream.";
        private const string TmplFailedMsg = "{0} failed: {1}";
        private const string TmplInitLacksSupport = "The stream cannot be initialized for {0} as the base stream does not support {1}.";
        private const string TmplInitInvalidParams = "Could not initialize {0} handle with the specified parameters.";
        private const string TmplReinitInvalidParams = "Could not reinitialize the {0} with the specified parameters.";

        private const int DefaultBufferSize = 4096;

        private readonly Stream _baseStream;
        private readonly bool _leaveOpen;
        private readonly byte[] _inputBuffer;
        private readonly byte[] _outputBuffer;
        private CompressionHandle? _compressionHandle;
        private DecompressionHandle? _decompressionHandle;
        private bool _compressionNeedsReinit = false;
        private DecompressionParameters _decompressionParameters;
        private bool _decompressionNeedsReinit = false;
        private int _bufferOffset = 0;

        [MemberNotNullWhen(true, nameof(_baseStream), nameof(_decompressionHandle))]
        public override bool CanRead => _baseStream?.CanRead is true && _decompressionHandle?.IsInvalid is false;

        [MemberNotNullWhen(true, nameof(_baseStream), nameof(_compressionHandle))]
        public override bool CanWrite => _baseStream?.CanWrite is true && _compressionHandle?.IsInvalid is false;

        public override bool CanSeek => false;

        public override long Length => _baseStream.Length;

        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }

        public Stream BaseStream => _baseStream;

        /// <summary>
        /// Initializes a new <see cref="LzhamStream"/> in compression-only mode using the specified stream.
        /// </summary>
        /// <param name="stream">The stream to use. The stream must support writing.</param>
        /// <param name="compressionParameters">The parameters to use to initialize the compressor.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LzhamStream"/> is disposed; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentException"/>
        public LzhamStream(Stream stream, CompressionParameters compressionParameters, bool leaveOpen = false)
            : this(stream, compressionParameters, null, leaveOpen) { }

        /// <summary>
        /// Initializes a new <see cref="LzhamStream"/> in decompression-only mode using the specified stream.
        /// </summary>
        /// <param name="stream">The stream to use. The stream must support reading.</param>
        /// <param name="decompressionParameters"></param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LzhamStream"/> is disposed; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentException"/>
        public LzhamStream(Stream stream, DecompressionParameters decompressionParameters, bool leaveOpen = false)
            : this(stream, null, decompressionParameters, leaveOpen) { }

        /// <summary>
        /// Initializes a new <see cref="LzhamStream"/> that supports compression and/or decompression using the specified stream.
        /// </summary>
        /// <param name="stream">The stream to use. The stream must support reading if <paramref name="decompressionParameters"/> is non-<see langword="null"/> and writing if <paramref name="compressionParameters"/> is non-<see langword="null"/>.</param>
        /// <param name="compressionParameters">The parameters to use to initialize the compressor. Must be non-<see langword="null"/> if <paramref name="decompressionParameters"/> is <see langword="null"/>.</param>
        /// <param name="decompressionParameters">The parameters to use to initialize the decompressor. Must be non-<see langword="null"/> if <paramref name="compressionParameters"/> is <see langword="null"/>.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LzhamStream"/> is disposed; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentException"/>
        public LzhamStream(Stream stream, CompressionParameters? compressionParameters, DecompressionParameters? decompressionParameters, bool leaveOpen = false)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            if (compressionParameters is null && decompressionParameters is null)
                throw new ArgumentException($"The stream must be initialized with valid {nameof(CompressionParameters)} or valid {nameof(DecompressionParameters)}.");

            if (compressionParameters is not null)
            {
                if (!stream.CanWrite)
                    throw new ArgumentException(string.Format(TmplInitLacksSupport, "compression", "writing"), nameof(stream));

                var compressionParametersRef = compressionParameters.Value;
                _compressionHandle = Lzham.CompressInit(compressionParametersRef);

                if (_compressionHandle.IsInvalid)
                    throw new ArgumentException(string.Format(TmplInitInvalidParams, "compression"), nameof(compressionParameters));
            }

            if (decompressionParameters is not null)
            {
                if (!stream.CanRead)
                    throw new ArgumentException(string.Format(TmplInitLacksSupport, "decompression", "reading"), nameof(stream));

                _decompressionParameters = decompressionParameters.Value;
                _decompressionHandle = Lzham.DecompressInit(_decompressionParameters);

                if (_decompressionHandle.IsInvalid)
                    throw new ArgumentException(string.Format(TmplInitInvalidParams, "decompression"), nameof(decompressionParameters));
            }

            _baseStream = stream;
            _leaveOpen = leaveOpen;
            _inputBuffer = new byte[DefaultBufferSize];
            _outputBuffer = new byte[DefaultBufferSize];
        }

        /// <summary>
        /// If this stream was initialized with compression parameters, manually reinitializes the underlying Lzham compressor. This is done automatically when needed, but you can manually use it here.
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="LzhamException"/>
        public void Reinit()
        {
            CheckCompressionSupported();
            _compressionHandle = Lzham.CompressReinit(_compressionHandle);

            if (_compressionHandle.IsInvalid) throw new LzhamException("Failed reinitializing the compressor.");
        }

        /// <summary>
        /// If this stream was initialized with compression parameters, manually reinitializes the underlying Lzham compressor using the given parameters. This is done automatically when needed, but you can manually use it here.
        /// <br/>
        /// NOTE: This does a full initialization by calling <see cref="Lzham.CompressInit(ref CompressionParameters)"/> as the reinitialization method does not support taking in new parameters!
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="LzhamException"/>
        public void Reinit(CompressionParameters compressionParameters)
        {
            CheckCompressionSupported();
            _compressionHandle = Lzham.CompressInit(compressionParameters);

            if (_compressionHandle.IsInvalid) throw new LzhamException(string.Format(TmplReinitInvalidParams, "compressor"));
        }

        /// <summary>
        /// If this stream was initialized with decompression parameters, manually reinitializes the underlying Lzham decompressor using the given parameters. This is done automatically when needed, but you can manually use it here.
        /// </summary>
        /// <param name="decompressionParameters">The new parameters to use.</param>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="LzhamException"/>
        public void Reinit(DecompressionParameters decompressionParameters)
        {
            CheckDecompressionSupported();

            _decompressionHandle = Lzham.DecompressReinit(_decompressionHandle, decompressionParameters);
            _decompressionParameters = decompressionParameters;

            if (_decompressionHandle.IsInvalid) throw new LzhamException(string.Format(TmplReinitInvalidParams, "decompressor"));
        }

        /// <summary>
        /// Reads a sequence of compressed bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">
        ///     An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/>
        ///     and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read and decompressed from the current source.</param>
        /// <param name="offset"><inheritdoc path="//param[2]"/></param>
        /// <param name="count"><inheritdoc path="//param[3]"/></param>
        /// <returns>The total number of bytes that were decompressed.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer is null) throw new ArgumentNullException(nameof(buffer));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (offset < 0 || (buffer.Length - offset < count)) throw new ArgumentOutOfRangeException(nameof(offset));

            CheckDecompressionSupported();
            if (_decompressionNeedsReinit) Reinit(_decompressionParameters);

            int totalBytesRead = 0;
            int totalDecompressedBytes = 0;
            int bytesLeftFromStream = 0;
            bool finishReading = false;
            _bufferOffset = 0;
            DecompressStatus status;
            do
            {
                if (!finishReading)
                {
                    bytesLeftFromStream = _baseStream.Read(_inputBuffer, 0, count);
                    _bufferOffset = 0;
                }
                finishReading = finishReading || bytesLeftFromStream == 0;

                do
                {
                    IntPtr compressedBytesLength = new IntPtr(bytesLeftFromStream); // Equal to the number of bytes we have read from the stream.
                    IntPtr outputBufferLength = new IntPtr(_outputBuffer.Length);

                    status = Lzham.Decompress(_decompressionHandle, _inputBuffer, ref compressedBytesLength, _bufferOffset, _outputBuffer, ref outputBufferLength, 0, finishReading);

                    int decompressedBytesWritten = outputBufferLength.ToInt32();
                    Array.Copy(_outputBuffer, 0, buffer, offset, decompressedBytesWritten);
                    totalDecompressedBytes += decompressedBytesWritten;
                    offset += decompressedBytesWritten;

                    int compressedBytesRead = compressedBytesLength.ToInt32();
                    _bufferOffset += compressedBytesRead; // Increment offset in compressed bytes buffer by number of bytes read from it
                    totalBytesRead += compressedBytesRead;
                    bytesLeftFromStream -= compressedBytesRead;
                }
                while (status == DecompressStatus.HasMoreOutput && totalBytesRead < count);
            }
            while (status < DecompressStatus.FirstSuccessOrFailureCode && totalBytesRead < count && bytesLeftFromStream > 0);

            if (status != DecompressStatus.Success)
            {
                throw new LzhamException(string.Format(TmplFailedMsg, "Decompression", status));
            }

            _decompressionNeedsReinit = true;
            return totalDecompressedBytes;
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Write(buffer.ToArray(), 0, buffer.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(buffer, offset, count, false);
        }

        public void Write(byte[] buffer, int offset, int count, bool finishing)
        {
            if (buffer is null) throw new ArgumentNullException(nameof(buffer));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (offset < 0 || (buffer.Length - offset < count)) throw new ArgumentOutOfRangeException(nameof(offset));

            CheckCompressionSupported();
            if (_compressionNeedsReinit) Reinit();

            int totalCompressedBytes = 0;
            int bytesLeftFromBuffer = count;
            bool finishWriting = false;
            _bufferOffset = 0;
            CompressStatus status;
            do
            {
                do
                {
                    finishWriting = finishWriting || bytesLeftFromBuffer == 0;

                    IntPtr decompressedBytesLength = new IntPtr(bytesLeftFromBuffer); // Equal to the number of bytes we have left from the buffer.
                    IntPtr outputBufferLength = new IntPtr(_outputBuffer.Length);

                    Debug.WriteLine($"Decomp bytes length: {decompressedBytesLength.ToInt32()}");
                    Debug.WriteLine($"Output buffer length: {outputBufferLength.ToInt32()}");

                    Debug.WriteLine($"Input (l {decompressedBytesLength.ToInt32()}, o {offset}): {string.Join(' ', buffer)}");

                    status = Lzham.Compress(_compressionHandle, buffer, ref decompressedBytesLength, offset, _outputBuffer, ref outputBufferLength, 0, finishWriting);

                    Debug.WriteLine($"Status: {status}");
                    Debug.WriteLine($"Output (l {outputBufferLength.ToInt32()}): {string.Join(' ', _outputBuffer[0..(outputBufferLength.ToInt32())])}");

                    int compressedBytesWritten = outputBufferLength.ToInt32();
                    totalCompressedBytes += compressedBytesWritten;
                    if (compressedBytesWritten > 0)
                        _baseStream.Write(_outputBuffer, 0, compressedBytesWritten);

                    int decompressedBytesRead = decompressedBytesLength.ToInt32();
                    offset += decompressedBytesRead; // Increment offset in decompressed bytes buffer by number of bytes read from it
                    bytesLeftFromBuffer -= decompressedBytesRead;
                }
                while (status == CompressStatus.HasMoreOutput);
            }
            while (status < CompressStatus.FirstSuccessOrFailureCode && bytesLeftFromBuffer > 0);

            if (status > CompressStatus.Success)
            {
                throw new LzhamException(string.Format(TmplFailedMsg, "Compression", status));
            }

            _compressionNeedsReinit = true;
        }

        /// <summary>
        /// Sets the position within the underlying stream, if it supports it.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the underlying stream, if it supports it.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override void Flush()
        {
            CheckForDisposal();
            _baseStream.Flush();
        }

        /// <summary>
        /// Deinitializes the <see cref="LzhamStream"/> compressor and decompressor, returning adler32 values with non-<see langword="null"/> values indicating success.
        /// <br/>
        /// Called automatically once this <see cref="LzhamStream"/> is disposed. Does NOT call <see cref="Stream.Close()"/>!
        /// </summary>
        /// <returns></returns>
        public void Deinit(out uint? compressionAdler32, out uint? decompressionAdler32)
        {
            compressionAdler32 = null;
            decompressionAdler32 = null;

            if (_compressionHandle?.IsInvalid == false)
            {
                compressionAdler32 = _compressionHandle.Deinit();
            }
            if (_decompressionHandle?.IsInvalid == false)
            {
                decompressionAdler32 = _decompressionHandle.Deinit();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _compressionHandle?.Deinit();
                _decompressionHandle?.Deinit();
                if (!_leaveOpen) _baseStream?.Dispose();
            }
        }

        private void CheckForDisposal()
        {
            if (_baseStream is null) throw new ObjectDisposedException(nameof(BaseStream), "The underlying stream was disposed.");
        }

        [MemberNotNull(nameof(_compressionHandle))]
        private void CheckCompressionSupported()
        {
            CheckForDisposal();

            if (_compressionHandle is null)
                throw new NotSupportedException(string.Format(TmplSingleModeMsg, "Writing", "decompression-only"));

            if (_compressionHandle.IsInvalid || _compressionHandle.IsClosed)
                throw new LzhamException(string.Format(TemplateHandleClosedMsg, "compressor"));
        }

        [MemberNotNull(nameof(_decompressionHandle))]
        private void CheckDecompressionSupported()
        {
            CheckForDisposal();
            if (_decompressionHandle is null)
                throw new NotSupportedException(string.Format(TmplSingleModeMsg, "Reading", "compression-only"));

            if (_decompressionHandle.IsInvalid || _decompressionHandle.IsClosed)
                throw new LzhamException(string.Format(TemplateHandleClosedMsg, "decompressor"));
        }
    }
}