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
            _handle = Lzham.DecompressInit(parameters);
            _parameters = parameters;

            if (_handle.IsInvalid)
                throw new LzhamException(MsgHandleInitFailed);
        }

        /// <summary>
        /// Reinitializes the decompressor using the specified parameters.
        /// </summary>
        public virtual void Reinit(DecompressionParameters parameters)
        {
            ValidateHandle();

            var newHandle = Lzham.DecompressReinit(_handle, parameters);
            if (_handle.DangerousGetHandle() != newHandle.DangerousGetHandle())
                _handle.Deinit();

            _handle = newHandle;
            _needsReinit = false;

            if (_handle.IsInvalid)
                throw new LzhamException(MsgHandleInitFailed);
        }

        /// <summary>
        /// Reinitializes the decompressor using the original parameters. This is called automatically when needed.
        /// </summary>
        public virtual void Reinit()
        {
            Reinit(_parameters);
        }

        /// <summary>
        /// Decompresses the bytes in <paramref name="input"/> and reads them into <paramref name="output"/>.
        /// </summary>
        /// <param name="input">The compressed data to be decompressed. Must be exactly the size of the compressed data.</param>
        /// <param name="output">The buffer to read the decompressed data into.</param>
        /// <returns>The size of the decompressed data.</returns>
        public virtual uint Decompress(ReadOnlySpan<byte> input, Span<byte> output)
        {
            if (_needsReinit) Reinit();

            uint totalInput = 0;
            uint totalOutput = 0;
            DecompressStatus status;
            // Could be more efficient if I did buffers and used the 'noMoreInputBytesFlag' value properly, but eh.
            do
            {
                nuint inCount = (uint)input.Length;
                nuint outCount = (uint)output.Length;
                status = Lzham.Decompress(_handle, input, ref inCount, output, ref outCount, true);

                totalInput += (uint)inCount;
                totalOutput += (uint)outCount;
            }
            while (status < DecompressStatus.FirstSuccessOrFailureCode && totalInput < input.Length && totalOutput < output.Length);

            if (status != DecompressStatus.Success)
                throw new LzhamException($"Decompression failed: {status}");

            _needsReinit = true;
            return totalOutput;
        }

        /// <summary>
        /// Decompresses the bytes in <paramref name="input"/> and reads them into <paramref name="output"/>, 
        /// starting at the appropriate offsets and for the specified number of bytes for each.
        /// </summary>
        /// <param name="input">The compressed data to be decompressed. Must be exactly the size of the compressed data.</param>
        /// <param name="inputOffset">The offset to start at in <paramref name="input"/>.</param>
        /// <param name="inputCount">The number of bytes to read.</param>
        /// <param name="output">The buffer to read the decompressed data into.</param>
        /// <param name="outputOffset">The offset to start at in <paramref name="output"/>.</param>
        /// <param name="inputCount">The number of bytes to write.</param>
        /// <returns><inheritdoc cref="Decompress(ReadOnlySpan{byte}, Span{byte})"/></returns>
        public void Decompress(byte[] input, int inputOffset, int inputCount, byte[] output, int outputOffset, int outputCount)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (output is null)
                throw new ArgumentNullException(nameof(output));

            if (inputCount < 0)
                throw new ArgumentOutOfRangeException(nameof(inputCount));
            if (outputCount < 0)
                throw new ArgumentOutOfRangeException(nameof(outputCount));

            if (inputOffset < 0 || input.Length - inputOffset < inputCount)
                throw new ArgumentOutOfRangeException(nameof(inputOffset));
            if (outputOffset < 0 || output.Length - outputOffset < outputOffset)
                throw new ArgumentOutOfRangeException(nameof(outputOffset));

            Decompress(input.AsSpan(inputOffset, inputCount), output.AsSpan(outputOffset, outputCount));
        }

        public void Dispose()
        {
            _handle.Dispose();
            GC.SuppressFinalize(this);
        }

        private void ValidateHandle()
        {
            if (_handle.IsClosed || _handle.IsInvalid)
                throw new LzhamException("The handle to the decompressor has been closed or is invalid.");
        }
    }
}