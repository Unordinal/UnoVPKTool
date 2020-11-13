using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnoVPKTool.VPK;

namespace UnoVPKTool.Extensions
{
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a null-terminated string from the current stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadNullTermString(this BinaryReader reader)
        {
            StringBuilder sb = new StringBuilder();
            char c;
            try
            {
                while ((c = reader.ReadChar()) != char.MinValue)
                {
                    sb.Append(c);
                }
            }
            catch (Exception)
            {

            }

            return sb.ToString();
        }

        /// <summary>
        /// Reads a <see cref="Tree"/> from the underlying stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Tree ReadTree(this BinaryReader reader)
        {
            return new Tree(reader);
        }

        /// <summary>
        /// Reads an <see cref="Entry"/> from the underlying stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Entry ReadEntry(this BinaryReader reader)
        {
            return new Entry(reader);
        }

        /// <summary>
        /// Reads a sequence of entries from the underlying stream until an <see cref="EntryBlock.Terminator"/> is reached.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns></returns>
        public static IEnumerable<Entry> ReadEntries(this BinaryReader reader)
        {
            do
            {
                yield return reader.ReadEntry();
            }
            while (reader.ReadUInt16() != EntryBlock.Terminator);
        }
    }
}
