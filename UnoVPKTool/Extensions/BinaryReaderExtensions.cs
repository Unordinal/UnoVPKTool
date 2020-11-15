using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static IEnumerable<DirectoryEntryBlock> ReadEntryBlocks(this BinaryReader reader)
        {
            string extension, path, name;
            while (!string.IsNullOrEmpty(extension = reader.ReadNullTermString()))
            {
                while (!string.IsNullOrEmpty(path = reader.ReadNullTermString()))
                {
                    while (!string.IsNullOrEmpty(name = reader.ReadNullTermString()))
                    {
                        var fullPath = Utils.GetFilePathFromVPKParts(extension, path, name);
                        yield return new DirectoryEntryBlock(reader, fullPath);
                    }
                }
            }
        }

        /// <summary>
        /// Reads a sequence of entries from the underlying stream until an <see cref="DirectoryEntryBlock.Terminator"/> is reached.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns></returns>
        public static IEnumerable<DirectoryEntry> ReadEntries(this BinaryReader reader)
        {
            do
            {
                yield return new DirectoryEntry(reader);
            }
            while (reader.ReadUInt16() != DirectoryEntryBlock.Terminator);
        }
    }
}