using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using CommandLine.Text;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.Parser.TextureArchive;
using Sewer56.SonicRiders.Parser.TextureArchive.Structs;
using File = System.IO.File;

namespace RidersTextureArchiveTool
{
    class Program
    {
        private const string FileOrderFileName = "order.txt";

        static void Main(string[] args)
        {
            var parser = new Parser(with =>
            {
                with.AutoHelp = true;
                with.CaseSensitive = false;
                with.CaseInsensitiveEnumValues = true;
                with.EnableDashDash = true;
                with.HelpWriter = null;
            });

            var parserResult = parser.ParseArguments<ExtractOptions, PackOptions, PackAllOptions>(args);
            parserResult.WithParsed<ExtractOptions>(Extract)
                        .WithParsed<PackOptions>(Pack)
                        .WithParsed<PackAllOptions>(PackAll)
                        .WithNotParsed(errs => HandleParseError(parserResult, errs));
        }

        private static void PackAll(PackAllOptions packAllOptions)
        {
            var sources = File.ReadAllLines(packAllOptions.Sources);
            var paths   = File.ReadAllLines(packAllOptions.SavePaths);

            if (sources.Length != paths.Length)
                throw new ArgumentException("Amount of source folders does not equal amount of save paths.");

            for (int x = 0; x < sources.Length; x++)
            {
                Console.WriteLine($"Saving: {paths[x]}");
                Pack(new PackOptions() { SavePath = paths[x], Source = sources[x], BigEndian = packAllOptions.BigEndian });
            }
        }

        private static void Pack(PackOptions options)
        {
            var writer         = new TextureArchiveWriter();
            string[] fileOrder = File.ReadAllLines(Path.Combine(options.Source, FileOrderFileName));

            foreach (var file in fileOrder)
            {
                var filePath = Path.Combine(options.Source, file);
                var fileName = Path.GetFileName(filePath).UnicodeUnReplaceSlash();
                var fileData = File.ReadAllBytes(filePath);
                writer.AddFile(fileName, fileData);
            }

            // Write file to new location.
            Directory.CreateDirectory(Path.GetDirectoryName(options.SavePath));
            using var fileStream = new FileStream(options.SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
            writer.Write(fileStream, options.BigEndian ? TextureArchiveWriterSettings.GameCube : TextureArchiveWriterSettings.PC);
        }

        private static void Extract(ExtractOptions options)
        {
            using var fileStream    = new FileStream(options.Source, FileMode.Open, FileAccess.Read);
            using var archiveReader = new TextureArchiveReader(fileStream, (int) fileStream.Length, options.BigEndian);
            Directory.CreateDirectory(options.SavePath);
            List<string> fileNames = new List<string>();


            for (var x = 0; x < archiveReader.Files.Length; x++)
            {
                ref var file = ref archiveReader.Files[x];
                var replacedFileName = file.Name.UnicodeReplaceSlash();

                fileNames.Add(replacedFileName);
                var filePath = Path.Combine(options.SavePath, replacedFileName);

                File.WriteAllBytes(filePath, archiveReader.GetFile(file));
                Console.WriteLine($"Writing {filePath}");
            }

            File.WriteAllLines(Path.Combine(options.SavePath, FileOrderFileName), fileNames);
        }

        /// <summary>
        /// Errors or --help or --version.
        /// </summary>
        static void HandleParseError(ParserResult<object> options, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(options, help =>
            {
                help.Copyright = "Created by Sewer56, licensed under GNU LGPL V3";
                help.AutoHelp = false;
                help.AutoVersion = false;
                help.AddDashesToOption = true;
                help.AddEnumValuesToHelpText = true;
                help.AdditionalNewLineAfterOption = true;
                return HelpText.DefaultParsingErrorsHandler(options, help);
            }, example => example, true);

            Console.WriteLine(helpText);
        }
    }
}
