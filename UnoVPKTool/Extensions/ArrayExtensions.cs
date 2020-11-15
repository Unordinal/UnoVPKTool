using System;
using System.IO;
using System.Linq;

namespace UnoVPKTool.Extensions
{
    public static class ArrayExtensions
    {
        public static BinaryReader GetReaderOverArray(this byte[] array)
        {
            return new BinaryReader(new MemoryStream(array));
        }

        // Taken from https://www.techiedelight.com/concatenate-byte-arrays-csharp/
        public static byte[] Combine(this byte[][] arrays)
        {
            byte[] output = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, output, offset, array.Length);
                offset += array.Length;
            }

            return output;
        }
    }
}
