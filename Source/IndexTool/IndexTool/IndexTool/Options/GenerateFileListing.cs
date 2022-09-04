using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IndexTool.Misc;
using IndexTool.Options.Interfaces;
using IndexTool.Services;
using IndexTool.Structs;
using IndexTool.Templates;
using Sewer56.SonicRiders.Parser.File;

namespace IndexTool.Options;

public class GenerateFileListing : IOption
{
    public static readonly string OutputPath     = $"{Path.GetDirectoryName(typeof(TemplateGenerator).Assembly.Location)}/out/GenerateFileListing/Files.md";
    public static readonly string TemplateFolder = $"ListFilesTable";

    public string GetName() => "Generate File Listing";

    public void Execute()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

        string path = Path.GetFullPath(Utilities.GetValidDirectory("Full Path to PC Riders' Data Folder"));
        var files   = GetFiles(path, out int totalFiles, out int guessedFiles);
        files.Sort((x, y) => x.Path.CompareTo(y.Path));

        Console.WriteLine("JSON: ");
        Console.WriteLine(Utilities.ToJson(files));

        Console.WriteLine("Template: ");
        var generator = new TemplateGenerator(TemplateFolder);
        var text      = generator.Generate(new { files });
        Console.WriteLine(text);

        Console.WriteLine("Writing File: ");
        File.WriteAllText(OutputPath, text);

        try
        {
            Console.WriteLine($"Your files are ready at: {OutputPath}");
            Process.Start(new ProcessStartInfo("cmd", $"/c start explorer \"{Path.GetFullPath(OutputPath)}\""));
        }
        catch (Exception)
        {

        }

        Console.WriteLine($"Guessed Formats Count: {guessedFiles}/{totalFiles}");
    }

    public List<DataFile> GetFiles(string dataFolder, out int totalFiles, out int guessedFiles)
    {
        var files = Directory.GetFiles(dataFolder, "*.*", SearchOption.AllDirectories);
        var result = new List<DataFile>();

        // Go through each file.
        totalFiles   = files.Length;
        guessedFiles = 0;

        foreach (var file in files)
        {
            using var data = File.OpenRead(file);
            if (FileTypeGuesser.TryGuess(data, (int) data.Length, out var type))
                guessedFiles += 1;

            result.Add(new DataFile()
            {
                Size = Utilities.ToUserFriendlyFileSize(data.Length),
                Path = file.Substring(dataFolder.Length + 1),
                Type = type,
                Description = FileDescriptionService.TryDescribe(Path.GetFileName(file))
            });
        }

        return result;
    }
}