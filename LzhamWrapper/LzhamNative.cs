using System.Runtime.InteropServices;

namespace LzhamWrapper
{
    /// <summary>
    /// P/Invoke for LZHAM .dll methods.
    /// </summary>
    internal static partial class LzhamNative
    {
        private const string LzhamDll = "lzham_x64.dll";

        /// <summary>
        /// Gets the version of the LZHAM .dll.
        /// </summary>
        /// <returns>The version of the LZHAM .dll.</returns>
        [DllImport(LzhamDll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "lzham_get_version")]
        internal static extern uint GetVersion();
    }
}