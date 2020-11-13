/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 *
 */

using System;

namespace XCompression
{
    public abstract class BaseContext : IDisposable
    {
        internal IntPtr XnaNativeHandle;

        internal Delegates.CreateCompressionContext NativeCreateCompressionContext;

        internal Delegates.Compress NativeCompress;

        internal Delegates.DestroyCompressionContext NativeDestroyCompressionContext;

        internal Delegates.CreateDecompressionContext NativeCreateDecompressionContext;

        internal Delegates.Decompress NativeDecompress;

        internal Delegates.DestroyDecompressionContext NativeDestroyDecompressionContext;

        internal BaseContext()
        {
            if (XnaNative.Load(this) == false)
            {
                throw new InvalidOperationException();
            }
        }

        ~BaseContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal ErrorCode CreateCompressionContext(int type, CompressionSettings settings, int flags, out IntPtr context)
        {
            if (XnaNativeHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("XnaNative is not loaded");
            }

            context = IntPtr.Zero;
            return (ErrorCode)NativeCreateCompressionContext(1, ref settings, flags, ref context);
        }

        internal ErrorCode Compress(IntPtr context, IntPtr output, ref int outputSize, IntPtr input, ref int inputSize)
        {
            if (XnaNativeHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("XnaNative is not loaded");
            }

            return (ErrorCode)NativeCompress(context, output, ref outputSize, input, ref inputSize);
        }

        internal void DestroyCompressionContext(IntPtr context)
        {
            if (XnaNativeHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("XnaNative is not loaded");
            }

            NativeDestroyCompressionContext(context);
        }

        internal ErrorCode CreateDecompressionContext(int type, CompressionSettings settings, int flags, out IntPtr context)
        {
            if (XnaNativeHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("XnaNative is not loaded");
            }

            context = IntPtr.Zero;
            return (ErrorCode)NativeCreateDecompressionContext(1, ref settings, flags, ref context);
        }

        internal ErrorCode Decompress(IntPtr context, IntPtr output, ref int outputSize, IntPtr input, ref int inputSize)
        {
            if (XnaNativeHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("XnaNative is not loaded");
            }

            return (ErrorCode)NativeDecompress(context, output, ref outputSize, input, ref inputSize);
        }

        internal void DestroyDecompressionContext(IntPtr context)
        {
            if (XnaNativeHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("XnaNative is not loaded");
            }

            NativeDestroyDecompressionContext(context);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (XnaNativeHandle != IntPtr.Zero)
            {
                NativeCreateCompressionContext = null;
                NativeCompress = null;
                NativeDestroyCompressionContext = null;
                NativeCreateDecompressionContext = null;
                NativeDecompress = null;
                NativeDestroyDecompressionContext = null;
                //todo: add NativeContext
                //Kernel32.FreeLibrary(this.XnaNativeHandle);
                XnaNativeHandle = IntPtr.Zero;
            }
        }
    }
}