using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IndexTool.Misc;
using IndexTool.Options.Interfaces;
using IndexTool.Structs;
using IndexTool.Templates;
using Mapster;
using Sewer56.SonicRiders.Parser.Archive;
using Sewer56.SonicRiders.Parser.File;
using Sewer56.SonicRiders.Parser.Layout.Enums;

namespace IndexTool.Options;

public class ScanArchiveData : IOption
{
    public static readonly string OutputPath          = $"{Path.GetDirectoryName(typeof(TemplateGenerator).Assembly.Location)}/out/ScanArchiveData";
    public static readonly string TemplateTableFolder = $"ListInternalTypesTable";
    public static readonly string TemplateFileFolder  = $"ListInternalTypesFile";

    public static readonly string GroupOutputPath = Path.Combine(OutputPath, "groups");
    public static readonly string TableOutputPath = Path.Combine(OutputPath, "table.md");
    public static readonly string JsonOutputPath  = Path.Combine(OutputPath, "types.json");

    // Performance: Caching
    private readonly HashSet<ObjectId> _objectIds = Enum.GetValues<ObjectId>().ToHashSet();
        
    public string GetName() => "Scan Riders Archives";
    public void Execute()
    {
        Directory.CreateDirectory(OutputPath);
        Directory.CreateDirectory(GroupOutputPath);

        Console.WriteLine($"Note: You can override individual entries by modifying {nameof(KnownFileTypes.Types)}");
        string path = Path.GetFullPath(Utilities.GetValidDirectory("Full Path to PC Riders' Data Folder"));

        Console.WriteLine("This might take a while...");
        var types = GetInternalTypes(path);
            
        Console.WriteLine("Writing out Files: ");
        File.WriteAllText(JsonOutputPath, Utilities.ToJson(types));
        var fileGenerator = new TemplateGenerator(TemplateFileFolder);
        foreach (var type in types)
        {
            var filePath = Path.Combine(GroupOutputPath, $"{type.Id}.md");
            File.WriteAllText(filePath, fileGenerator.Generate(new { type }));
        }

        var tableGenerator = new TemplateGenerator(TemplateTableFolder);
        File.WriteAllText(TableOutputPath, tableGenerator.Generate(new { types }));
        Console.WriteLine($"Written Table to {TableOutputPath}");

        try
        {
            Console.WriteLine($"Your files are ready at: {OutputPath}");
            Process.Start(new ProcessStartInfo("cmd", $"/c start explorer \"{Path.GetFullPath(OutputPath)}\""));
        }
        catch (Exception)
        {

        }
    }

    public List<InternalFileType> GetInternalTypes(string path)
    {
        var files  = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(x => Path.GetExtension(x) == "").ToArray();
        var groups = new Dictionary<int, InternalFileType>();

        foreach (var file in files)
        {
            using var fileStream    = File.OpenRead(file);
            using var archiveReader = new ArchiveReader(fileStream, (int) fileStream.Length, false);

            // Collect group data.
            foreach (var group in archiveReader.Groups)
            {
                // Make new group if not previously done
                if (!groups.TryGetValue(group.Id, out var value))
                {
                    value = new InternalFileType()
                    {
                        Id = group.Id,
                        Name = TryGetName(group.Id),
                        SeenAt = new List<string>(),
                        InternalFormats = new List<InternalFileType.InternalFormat>(),
                    };

                    groups[group.Id] = value;
                }

                // Add seen marker.
                var seenAt = file.Substring(path.Length + 1);
                if (!value.SeenAt.Contains(seenAt))
                    value.SeenAt.Add(seenAt);

                for (var x = 0; x < @group.Files.Length; x++)
                {
                    if (value.InternalFormats.Find(format => format.Id == x) == null)
                    {
                        value.InternalFormats.Add(new InternalFileType.InternalFormat()
                        {
                            Id = x, 
                            Name = TryGetFileFormat(archiveReader.GetFile(group, x))
                        });
                    }
                }
            }
        }

        var types = groups.Select(x => x.Value).ToList();

        // Inject known types.
        var knownTypes = KnownInternalIds.Ids;
        // ReSharper disable PossibleInvalidOperationException
        var typesDict  = types.ToDictionary(type => type.Id.Value!);

        foreach (var knownType in knownTypes)
        {
            if (typesDict.TryGetValue(knownType.Id, out var type))
                knownType.Adapt(type);
        }

        types.Sort((x, y) => x.Id.Value.CompareTo(y.Id.Value));
        return types;
        // ReSharper restore PossibleInvalidOperationException
    }

    private string TryGetFileFormat(byte[] data)
    {
        using var memStream = new MemoryStream(data);
        if (FileTypeGuesser.TryGuess(memStream, (int) memStream.Length, out var type))
            return type!.Format;

        return "Unknown";
    }

    private string TryGetName(int id)
    {
        if (_objectIds.Contains((ObjectId) id))
            return Enum.GetName((ObjectId)id);

        return "";
    }
}