using System.IO;

namespace UnoVPKTool.VPK
{
    public interface IBinaryWritable
    {
        /// <summary>
        /// Writes this <see cref="IBinaryWritable"/> to the current stream and advances the stream position by the number of bytes written.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(BinaryWriter writer);
    }
}
