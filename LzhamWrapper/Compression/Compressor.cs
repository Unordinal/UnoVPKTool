using System;
using LzhamWrapper.Exceptions;

namespace LzhamWrapper.Compression
{
    /// <summary>
    /// A wrapper for the LZHAM block-by-block compressor.
    /// </summary>
    public class Compressor : IDisposable
    {
        private const string MsgHandleInitFailed = "Failed to initialize the compressor with the specified parameters.";

        protected CompressionHandle _handle;
        protected CompressionParameters _parameters;
        private bool _needsReinit = false;

        public Compressor(CompressionParameters parameters)
        {
            _handle = Lzham.Compression.Init(parameters);
            _parameters = parameters;

            if (!_handle.IsOpenAndValid)
                throw new LzhamException(MsgHandleInitFailed);
        }

        /// <summary>
        /// Reinitializes the compressor using the specified parameters.
        /// <br/>
        /// NOTE: LZHAM alpha8 does not support reinitializing the compressor with different parameters; this method will do an entire new initialization of the compressor!
        /// </summary>
        public virtual void Reinit(CompressionParameters parameters)
        {
            var newHandle = Lzham.Compression.Init(parameters);

            _handle = newHandle;
            _needsReinit = false;

            if (!_handle.IsOpenAndValid)
                throw new LzhamException(MsgHandleInitFailed);
        }

        /// <summary>
        /// Reinitializes the compressor using the original parameters. This is called automatically when needed.
        /// </summary>
        public virtual void Reinit()
        {
            ValidateHandle();

            var newHandle = Lzham.Compression.Reinit(_handle);
            if (_handle.DangerousGetHandle() != newHandle.DangerousGetHandle())
                _handle.Deinit();

            _handle = newHandle;
            _needsReinit = false;

            if (!_handle.IsOpenAndValid)
                throw new LzhamException(MsgHandleInitFailed);
        }

        /// <inheritdoc cref="Lzham.Compression.Compress(CompressionHandle, ReadOnlySpan{byte}, Span{byte}, bool)"/>
        ///
        public virtual CompressStatus Compress(ReadOnlySpan<byte> input, Span<byte> output, bool noMoreInputBytes)
        {
            return Compress(input, out _, output, out _, noMoreInputBytes);
        }

        /// <inheritdoc cref="Lzham.Compression.Compress(CompressionHandle, ReadOnlySpan{byte}, out int, Span{byte}, out int, bool)"/>
        ///
        public virtual CompressStatus Compress(ReadOnlySpan<byte> input, out int bytesRead, Span<byte> output, out int bytesWritten, bool noMoreInputBytes)
        {
            if (_needsReinit) Reinit();
            if (noMoreInputBytes) _needsReinit = true;

            return Lzham.Compression.Compress(_handle, input, out bytesRead, output, out bytesWritten, noMoreInputBytes);
        }

        /// <inheritdoc cref="Lzham.Compression.CompressMemory(CompressionParameters, ReadOnlySpan{byte}, Span{byte}, ref uint)"/>
        ///
        public virtual CompressStatus CompressMemory(ReadOnlySpan<byte> input, Span<byte> output, ref uint adler32)
        {
            return CompressMemory(input, out _, output, out _, ref adler32);
        }

        /// <inheritdoc cref="Lzham.Compression.CompressMemory(DecompressionParameters, ReadOnlySpan{byte}, out int, Span{byte}, out int, ref uint)"/>
        ///
        public virtual CompressStatus CompressMemory(ReadOnlySpan<byte> input, out int bytesRead, Span<byte> output, out int bytesWritten, ref uint adler32)
        {
            return Lzham.Compression.CompressMemory(_parameters, input, out bytesRead, output, out bytesWritten, ref adler32);
        }

        public void Dispose()
        {
            _handle.Dispose();
            GC.SuppressFinalize(this);
        }

        private void ValidateHandle()
        {
            if (!_handle.IsOpenAndValid)
                throw new ObjectDisposedException("The handle to the compressor has been closed or is invalid.");
        }
    }
}
