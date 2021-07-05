using System;
using System.IO;
using IndexTool.Misc;
using IndexTool.Options.Interfaces;
using IndexTool.Options.TypeGuesser.Interfaces;
using IndexTool.Services;
using IndexTool.Structs;
using Reloaded.Memory.Streams;

namespace IndexTool.Options
{
    public class GuessFileType : IOption
    {
        public static readonly IFileTypeGuesser[] Guessers = Utilities.MakeAllInstances<IFileTypeGuesser>();

        public string GetName() => "Guess File Type";

        public void Execute()
        {
            Console.WriteLine($"Note: You can add types by using {nameof(IFileTypeGuesser)} interface and adding Ids to " + KnownTypesService.KnownTypesPath);
            string path = Path.GetFullPath(Utilities.GetValidFile("Full Path to File"));

            using var fileStream = File.OpenRead(path);

            TryGuess(fileStream, (int) fileStream.Length, out var type);
            Console.WriteLine(Utilities.ToJson(type));
        }

        public bool TryGuess(Stream data, int streamLength, out FileType fileType)
        {
            var pos = data.Position;
            using var extendedStream = new BufferedStreamReader(data, 2048);
            var extendedStreamPos = extendedStream.Position();

            foreach (var guesser in Guessers)
            {
                extendedStream.Seek(extendedStreamPos, SeekOrigin.Begin);

                try
                {
                    if (guesser.TryGuess(extendedStream, streamLength, out var id))
                    {
                        fileType = KnownTypesService.FromId(id);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }

            fileType = new FileType() { Id = "UNK", Format = "Unknown" };
            data.Position = pos;
            return false;
        }
    }
}
