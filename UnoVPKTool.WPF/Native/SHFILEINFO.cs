using System;
using System.Runtime.InteropServices;

namespace UnoVPKTool.WPF.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public const int NameSize = 80;
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }
}
