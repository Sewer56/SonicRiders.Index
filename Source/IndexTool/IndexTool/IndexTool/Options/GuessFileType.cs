using System;
using System.IO;
using IndexTool.Misc;
using IndexTool.Options.Interfaces;
using Sewer56.SonicRiders.Parser.File;
using Sewer56.SonicRiders.Parser.File.Guessers.Interfaces;

namespace IndexTool.Options;

public class GuessFileType : IOption
{
    public static readonly IFileTypeGuesser[] Guessers = Utilities.MakeAllInstances<IFileTypeGuesser>();

    public string GetName() => "Guess File Type";

    public void Execute()
    {
        Console.WriteLine($"Note: You can add types by using {nameof(IFileTypeGuesser)} interface and adding Ids to {nameof(KnownFileTypes.Types)}.");
        string path = Path.GetFullPath(Utilities.GetValidFile("Full Path to File"));

        using var fileStream = File.OpenRead(path);

        FileTypeGuesser.TryGuess(fileStream, (int)fileStream.Length, out var type);
        Console.WriteLine(Utilities.ToJson(type));
    }
}