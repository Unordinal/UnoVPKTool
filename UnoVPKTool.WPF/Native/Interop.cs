using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace UnoVPKTool.WPF.Native
{
    public static class Interop
    {
        public enum FolderType { Closed, Open }

        public enum IconSize { Large, Small }

        [Flags]
        public enum SHGFIFlags : uint
        {
            SmallIcon = 0x1,
            OpenIcon = 0x2,
            UseFileAttributes = 0x10,
            Icon = 0x100
        }

        public const uint FileAttributeDirectory = 0x10;

        public static BitmapSource GetFileExtensionIcon(SHGFIFlags flags = SHGFIFlags.Icon | SHGFIFlags.UseFileAttributes)
        {
            SHFILEINFO shfi = new SHFILEINFO();

            IntPtr icon = SHGetFileInfo(@"C:\Windows", FileAttributeDirectory, ref shfi, (uint)Marshal.SizeOf<SHFILEINFO>(), flags);
            var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(icon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DestroyIcon(icon);

            return bitmapSource;
        }

        [DllImport("Shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, SHGFIFlags uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}