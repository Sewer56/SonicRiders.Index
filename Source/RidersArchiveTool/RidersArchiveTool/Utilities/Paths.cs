namespace RidersArchiveTool.Utilities
{
    internal static class Paths
    {
        /// <summary>
        /// Retrieves a relative path for a file given a folder name.
        /// </summary>
        /// <param name="fullPath">Full path for the file.</param>
        /// <param name="folderPath">The folder to get the path relative to.</param>
        public static string GetRelativePath(string fullPath, string folderPath) => fullPath.Substring(folderPath.Length + 1);

        /// <summary>
        /// Appends a relative path to a given folder.
        /// </summary>
        /// <param name="relativePath">Relative path to the folder.</param>
        /// <param name="folderPath">Path to the folder.</param>
        public static string AppendRelativePath(string relativePath, string folderPath) => folderPath + "/" + relativePath;
    }
}
