using System;
using System.IO;
using System.Linq;

namespace UnoVPKTool.VPK
{
    public static class Utils
    {
        /// <summary>
        /// Gets a path to a VPK archive using the given directory path and archive index.
        /// </summary>
        /// <param name="vpkDirectoryPath">The path to a VPK directory. (ex: <c>englishclient_frontend.bsp.pak000_dir.vpk</c>)</param>
        /// <param name="vpkArchiveIndex">The index of the VPK archive.</param>
        /// <param name="basePath">The base path to combine with the resulting string. This will be prefixed to <paramref name="vpkDirectoryPath"/>.</param>
        /// <returns>A path to the VPK archive. (ex: <c>C:\Games\Apex Legends\vpk\client_frontend.bsp.pak000_000.vpk</c>)</returns>
        public static string DirectoryPathToArchivePath(string vpkDirectoryPath, ushort vpkArchiveIndex, string basePath = "")
        {
            string pathWithoutLoc = StripLocalizationFromDirectoryPath(vpkDirectoryPath);
            string archivePath = pathWithoutLoc.Replace("_dir", $"_{vpkArchiveIndex:000}");

            return Path.Combine(basePath, archivePath);
        }

        /// <summary>
        /// Strips the localization string from the start of a VPK directory file's name.
        /// </summary>
        /// <param name="vpkDirectoryPath">The path to or the name of a VPK directory file.</param>
        /// <returns>
        /// The path of the VPK directory with the localization string stripped.
        /// <br/>
        /// "<c>englishclient_frontend.bsp.pak000_dir.vpk</c>" -> "<c>client_frontend.bsp.pak000_dir.vpk</c>"
        /// </returns>
        public static string StripLocalizationFromDirectoryPath(string vpkDirectoryPath)
        {
            string dir = Path.GetDirectoryName(vpkDirectoryPath) ?? string.Empty;
            string dirName = Path.GetFileName(vpkDirectoryPath);

            string[] locs = Enum.GetNames<VPKLocalization>();
            string toStrip = string.Empty;
            if (locs.Any(s => dirName.StartsWith(toStrip = s, StringComparison.OrdinalIgnoreCase)))
            {
                dirName = dirName[toStrip.Length..];
            }

            return Path.Combine(dir, dirName);
        }

        /// <summary>
        /// Builds a file's path from VPK parts.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <param name="path">The file path.</param>
        /// <param name="name">The file name.</param>
        /// <returns>A file path for a file within a VPK archive. (ex: <c>resource\localization\base_english.txt</c></returns>
        public static string GetFilePathFromVPKParts(string extension, string path, string name)
        {
            path = path.Replace('/', Path.DirectorySeparatorChar).Trim();
            if (path != string.Empty) path += Path.DirectorySeparatorChar;

            return path + $"{name}.{extension}";
        }
    }
}