using Reloaded.Memory.Streams;

namespace IndexTool.Options.TypeGuesser.Interfaces
{
    public interface IFileTypeGuesser
    {
        /// <summary>
        /// Retrieves an ID corresponding to those in KnownFileTypes.json
        /// </summary>
        string GetId();

        /// <summary>
        /// Tries to guess the type of file.
        /// </summary>
        /// <param name="data">Stream to read file data.</param>
        /// <param name="fileSize">File size, if known, else -1.</param>
        /// <param name="id">The file id.</param>
        bool TryGuess(BufferedStreamReader data, int fileSize, out string id);
    }
}