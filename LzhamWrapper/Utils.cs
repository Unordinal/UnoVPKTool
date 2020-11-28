using System;

namespace LzhamWrapper
{
    internal static class Utils
    {
        public static void ValidateArrayBounds<T>(T[] array, int offset, int count)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (array.Length - offset < count) throw new ArgumentOutOfRangeException(nameof(offset));
        }
    }
}