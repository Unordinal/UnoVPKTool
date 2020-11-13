using System.IO;
using System.Text;
using UnoVPKTool.VPK;

namespace UnoVPKTool.Extensions
{
    public static class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes a null-terminated string to this stream in the specified encoding, and advances the current position of the stream in accordance with the encoding used and the specific characters being written to the stream.
        /// </summary>
        /// <param name="writer">The writer to use to write to a stream.</param>
        /// <param name="value">The string to write.</param>
        /// <param name="encoding">The encoding to use for the writer. If <see langword="null"/>, uses <see cref="Encoding.UTF8"/>.</param>
        public static void WriteNullTermString(this BinaryWriter writer, string value, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;

            byte[] buffer = encoding.GetBytes(value + char.MinValue);
            writer.Write(buffer);
        }

        /// <summary>
        /// Writes the given <see cref="IBinaryWritable"/> to the stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this BinaryWriter writer, IBinaryWritable value)
        {
            value.Write(writer);
        }
    }
}