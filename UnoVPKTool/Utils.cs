using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace UnoVPKTool
{
    public static class Utils
    {
        /// <summary>
        /// Marshals a <see cref="byte"/>[] to an <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of struct to marshal to.</typeparam>
        /// <param name="bytes">The bytes to marshal from.</param>
        /// <param name="success">Returns true if the marshalling was a success.</param>
        /// <returns></returns>
        public static T ToStruct<T>(this byte[] bytes, out bool success) where T : struct
        {
            success = false;
            T output;

            GCHandle handle = default;
            try
            {
                handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                output = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                success = true;
            }
            finally { if (handle.IsAllocated) handle.Free(); }

            return output;
        }
        
        /// <summary>
        /// Marshals a <see cref="byte"/>[] to an <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of struct to marshal to.</typeparam>
        /// <param name="bytes">The bytes to marshal from.</param>
        /// <returns></returns>
        public static T ToStruct<T>(this byte[] bytes) where T : struct
        {
            return ToStruct<T>(bytes, out _);
        }

        /// <summary>
        /// Marshals a <typeparamref name="T"/> to a <see cref="byte"/>[].
        /// </summary>
        /// <typeparam name="T">The type of struct to marshal from.</typeparam>
        /// <param name="structure">The structure to marshal.</param>
        /// <param name="success">Returns true if the marshalling was a success.</param>
        /// <returns></returns>
        public static byte[] ToBytes<T>(this T structure, out bool success) where T : struct
        {
            success = false;
            int size = Marshal.SizeOf(structure);
            byte[] output = new byte[size];

            GCHandle handle = default;
            try
            {
                handle = GCHandle.Alloc(output, GCHandleType.Pinned);
                Marshal.StructureToPtr<T>(structure, handle.AddrOfPinnedObject(), false);
                success = true;
            }
            finally { if (handle.IsAllocated) handle.Free(); }

            return output;
        }
        
        /// <summary>
        /// Marshals a <typeparamref name="T"/> to a <see cref="byte"/>[].
        /// </summary>
        /// <typeparam name="T">The type of struct to marshal from.</typeparam>
        /// <param name="structure">The structure to marshal.</param>
        /// <returns></returns>
        public static byte[] ToBytes<T>(this T structure) where T : struct
        {
            return ToBytes(structure, out _);
        }

        /// <summary>
        /// Indents the given string (on all lines) a certain number of spaces.
        /// </summary>
        /// <param name="indentCount"></param>
        /// <returns></returns>
        public static string IndentString(string str, int indentCount = 4)
        {
            string indents = new string(' ', indentCount);
            return indents + str.Replace("\n", "\n" + indents);
        }

        public static string DumpInstanceFields(object obj)
        {
            Type objType = obj.GetType();
            var fields = objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            StringBuilder sb = new StringBuilder();
            string indent = new string(' ', 4);
            foreach (var field in fields)
            {
                sb.AppendLine(indent + $"{field.Name}: {field.GetValue(obj)}");
            }
            return sb.ToString();
        }
    }
}
