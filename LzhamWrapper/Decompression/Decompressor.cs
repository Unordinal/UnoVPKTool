using System;
using LzhamWrapper.Exceptions;

namespace LzhamWrapper.Decompression
{
    /// <summary>
    /// A wrapper for the LZHAM block-by-block decompressor.
    /// </summary>
    public class Decompressor : IDisposable
    {
        private const string MsgHandleInitFailed = "Failed to initialize the decompressor with the specified parameters.";

        protected DecompressionHandle _handle;
        protected DecompressionParameters _parameters;
        private bool _needsReinit = false;

        public Decompressor(DecompressionParameters parameters)
        {
            _handle = Lzham.Decompression.Init(parameters);
            _parameters = parameters;

            if (!_handle.IsOpenAndValid)
                throw new LzhamException(MsgHandleInitFailed);
        }

        /// <summary>
        /// Reinitializes the decompressor using the specified parameters.
        /// </summary>
        public virtual void Reinit(DecompressionParameters parameters)
        {
            ValidateHandle();

            var newHandle = Lzham.Decompression.Reinit(_handle, parameters);
            if (_handle.DangerousGetHandle() != newHandle.DangerousGetHandle())
                _handle.Deinit();

            _handle = newHandle;
            _needsReinit = false;

            if (!_handle.IsOpenAndValid)
                throw new LzhamException(MsgHandleInitFailed);
        }

        /// <summary>
        /// Reinitializes the decompressor using the original parameters. This is called automatically when needed.
        /// </summary>
        public virtual void Reinit()
        {
            Reinit(_parameters);
        }

        /// <inheritdoc cref="Lzham.Decompression.Decompress(DecompressionHandle, ReadOnlySpan{byte}, Span{byte}, bool)"/>
        ///
        public virtual DecompressStatus Decompress(ReadOnlySpan<byte> input, Span<byte> output, bool noMoreInputBytes)
        {
            return Decompress(input, out _, output, out _, noMoreInputBytes);
        }

        /// <inheritdoc cref="Lzham.Decompression.Decompress(DecompressionHandle, ReadOnlySpan{byte}, out int, Span{byte}, out int, bool)"/>
        ///
        public virtual DecompressStatus Decompress(ReadOnlySpan<byte> input, out int bytesRead, Span<byte> output, out int bytesWritten, bool noMoreInputBytes)
        {
            ValidateHandle();
            if (_needsReinit) Reinit();
            if (noMoreInputBytes) _needsReinit = true;
            return Lzham.Decompression.Decompress(_handle, input, out bytesRead, output, out bytesWritten, noMoreInputBytes);
        }

        /// <inheritdoc cref="Lzham.Decompression.DecompressMemory(DecompressionParameters, ReadOnlySpan{byte}, Span{byte}, ref uint)"/>
        ///
        public virtual DecompressStatus DecompressMemory(ReadOnlySpan<byte> input, Span<byte> output, ref uint adler32)
        {
            return DecompressMemory(input, out _, output, out _, ref adler32);
        }
        
        /// <inheritdoc cref="Lzham.Decompression.DecompressMemory(DecompressionParameters, ReadOnlySpan{byte}, out int, Span{byte}, out int, ref uint)"/>
        ///
        public virtual DecompressStatus DecompressMemory(ReadOnlySpan<byte> input, out int bytesRead, Span<byte> output, out int bytesWritten, ref uint adler32)
        {
            return Lzham.Decompression.DecompressMemory(_parameters, input, out bytesRead, output, out bytesWritten, ref adler32);
        }

        public void Dispose()
        {
            _handle.Dispose();
            GC.SuppressFinalize(this);
        }

        private void ValidateHandle()
        {
            if (!_handle.IsOpenAndValid)
                throw new ObjectDisposedException("The handle to the decompressor has been closed or is invalid.");
        }
    }
}