using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using CommandLine.Text;
using Sewer56.SonicRiders.Parser.Archive;
using Sewer56.SonicRiders.Parser.Archive.Structs.Managed;
using File = System.IO.File;

namespace RidersArchiveTool
{
    class Program
    {
        private const char GroupIdSeparator = '_';

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
            var directories = Directory.GetDirectories(options.Source);
            var writer      = new ArchiveWriter();

            foreach (var dir in directories)
            {
                var directoryName = Path.GetFileNameWithoutExtension(dir).Split(GroupIdSeparator);

                var groupNo = Convert.ToByte(directoryName[0]);
                var id      = Convert.ToUInt16(directoryName[1]);
                var filesInside = Directory.GetFiles(dir);

                var group = new ManagedGroup() { Id = id, Files = new List<ManagedFile>(filesInside.Length) };
                foreach (var file in filesInside)
                    group.Files.Add(new ManagedFile() { Data = File.ReadAllBytes(file) });

                writer.AddGroup(groupNo, group);
            }

            // Write file to new location.
            Directory.CreateDirectory(Path.GetDirectoryName(options.SavePath));
            using var fileStream = new FileStream(options.SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
            writer.Write(fileStream, options.BigEndian);
        }

        private static void Extract(ExtractOptions options)
        {
            Directory.CreateDirectory(options.SavePath);

            using var fileStream   = new FileStream(options.Source, FileMode.Open, FileAccess.Read);
            using var archiveReader = new ArchiveReader(fileStream, (int) fileStream.Length, options.BigEndian);
            var allGroups = archiveReader.GetAllGroups();

            for (var x = 0; x < allGroups.Length; x++)
            {
                var group  = allGroups[x];
                var folder = Path.Combine(options.SavePath, $"{x:000}{GroupIdSeparator}{group.Id:00000}");
                Directory.CreateDirectory(folder);

                for (var y = 0; y < group.Files.Count; y++)
                {
                    var filePath = Path.Combine(folder, y.ToString("00000"));
                    File.WriteAllBytes(filePath, group.Files[y].Data);
                    Console.WriteLine($"Writing {filePath}");
                }
            }
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
