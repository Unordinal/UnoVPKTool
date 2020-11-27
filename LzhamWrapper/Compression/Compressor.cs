using System;
using LzhamWrapper.Exceptions;

namespace LzhamWrapper.Compression
{
    public class Compressor : IDisposable
    {
        private const string MsgHandleInitFailed = "Failed to initialize the compressor with the specified parameters.";

        protected CompressionHandle _handle;
        protected CompressionParameters _parameters;
        private bool _needsReinit = false;

        public Compressor(CompressionParameters parameters)
        {
            _handle = Lzham.CompressInit(parameters);
            _parameters = parameters;

            if (_handle.IsInvalid)
                throw new LzhamException(MsgHandleInitFailed);
        }

        /// <summary>
        /// Reinitializes the compressor using the specified parameters.
        /// <br/>
        /// NOTE: LZHAM alpha8 does not support reinitializing the compressor with different parameters; this method will do an entire new initialization of the compressor!
        /// </summary>
        public virtual void Reinit(CompressionParameters parameters)
        {
            var newHandle = Lzham.CompressInit(parameters);

            _handle = newHandle;
            _needsReinit = false;

            if (_handle.IsInvalid)
                throw new LzhamException(MsgHandleInitFailed);
        }

        /// <summary>
        /// Reinitializes the compressor using the original parameters. This is called automatically when needed.
        /// </summary>
        public virtual void Reinit()
        {
            ValidateHandle();

            var newHandle = Lzham.CompressReinit(_handle);
            if (_handle.DangerousGetHandle() != newHandle.DangerousGetHandle())
                _handle.Deinit();

            _handle = newHandle;
            _needsReinit = false;

            if (_handle.IsInvalid)
                throw new LzhamException(MsgHandleInitFailed);
        }

        /// <summary>
        /// Compresses the bytes in <paramref name="input"/> and reads them into <paramref name="output"/>.
        /// </summary>
        /// <param name="input">The decompressed data to be compressed. Must be exactly the size of the decompressed data.</param>
        /// <param name="output">The buffer to read the compressed data into.</param>
        /// <returns>The size of the compressed data.</returns>
        public virtual uint Compress(ReadOnlySpan<byte> input, Span<byte> output)
        {
            if (_needsReinit) Reinit();

            uint totalInput = 0;
            uint totalOutput = 0;
            CompressStatus status;
            do
            {
                nuint inCount = (uint)input.Length;
                nuint outCount = (uint)output.Length;
                status = Lzham.Compress(_handle, input, ref inCount, output, ref outCount, true);

                totalInput += (uint)inCount;
                totalOutput += (uint)outCount;
            }
            while (status < CompressStatus.FirstSuccessOrFailureCode && totalInput < input.Length && totalOutput < output.Length);

            if (status != CompressStatus.Success)
                throw new LzhamException($"Compression failed: {status}");

            _needsReinit = true;
            return totalOutput;
        }

        /// <summary>
        /// Compresses the bytes in <paramref name="input"/> and reads them into <paramref name="output"/>, 
        /// starting at the appropriate offsets and for the specified number of bytes for each.
        /// </summary>
        /// <param name="input">The decompressed data to be compressed.</param>
        /// <param name="inputOffset">The offset to start at in <paramref name="input"/>.</param>
        /// <param name="inputCount">The number of bytes to read.</param>
        /// <param name="output">The buffer to read the compressed data into.</param>
        /// <param name="outputOffset">The offset to start at in <paramref name="output"/>.</param>
        /// <param name="inputCount">The number of bytes to write.</param>
        /// <returns><inheritdoc cref="Compress(ReadOnlySpan{byte}, Span{byte})"/></returns>
        public void Compress(byte[] input, int inputOffset, int inputCount, byte[] output, int outputOffset, int outputCount)
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

            Compress(input.AsSpan(inputOffset, inputCount), output.AsSpan(outputOffset, outputCount));
        }

        public void Dispose()
        {
            _handle.Dispose();
            GC.SuppressFinalize(this);
        }

        private void ValidateHandle()
        {
            if (_handle.IsClosed || _handle.IsInvalid)
                throw new LzhamException("The handle to the compressor has been closed or is invalid.");
        }
    }
}
