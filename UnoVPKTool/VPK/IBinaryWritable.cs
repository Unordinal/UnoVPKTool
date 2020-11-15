using System.IO;

namespace UnoVPKTool.VPK
{
    /// <summary>
    /// A simple interface for an object that is able to be written to bytes via a <see cref="BinaryWriter"/>.
    /// </summary>
    public interface IBinaryWritable
    {
        /// <summary>
        /// Writes this <see cref="IBinaryWritable"/> to the underlying stream of the given <see cref="BinaryWriter"/> and advances the stream position by the number of bytes written.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(BinaryWriter writer);
    }
}