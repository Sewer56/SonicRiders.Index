using System.Collections.Generic;
using System.IO;

namespace RidersArchiveTool.Utilities
{
    internal static class IOUtilities
    {
        /// <summary>
        /// Gets the file size of a file with a specific path.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>Size of the file.</returns>
        public static long GetFileSize(string path) => new FileInfo(path).Length;

        /// <summary>
        /// Gets the file size of a set of files.
        /// </summary>
        /// <param name="files">Paths to the files.</param>
        /// <returns>Size of the files.</returns>
        public static long GetFileSize(IEnumerable<string> files)
        {
            long size = 0;
            foreach (var file in files)
                size += IOUtilities.GetFileSize(file);

            return size;
        }
    }
}
